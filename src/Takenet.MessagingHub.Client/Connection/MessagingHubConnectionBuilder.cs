using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Network.Modules;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Transport.Tcp;
using Takenet.MessagingHub.Client.LimeProtocol;

namespace Takenet.MessagingHub.Client.Connection
{
    public class MessagingHubConnectionBuilder<TConfigurator> : MessagingHubConnectionConfigurator<TConfigurator>
        where TConfigurator : MessagingHubConnectionBuilder<TConfigurator>
    {
        private readonly ITransportFactory _transportFactory;

        public MessagingHubConnectionBuilder(ITransportFactory transportFactory)
        {
            _transportFactory = transportFactory;
        }

        /// <summary>
        /// Builds a <see cref="IMessagingHubConnection">connection</see> with the configured parameters
        /// </summary>
        /// <returns>An inactive connection with the Messaging Hub. Call <see cref="IMessagingHubConnection.ConnectAsync"/> to activate it</returns>
        public IMessagingHubConnection Build()
        {
            var channelBuilder = ClientChannelBuilder.Create(() => _transportFactory.Create(EndPoint), EndPoint)
                                 .WithSendTimeout(SendTimeout)
                                 .WithEnvelopeBufferSize(100)
                                 .AddCommandModule(c => new ReplyPingChannelModule(c));

            channelBuilder =
                channelBuilder.AddBuiltHandler(
                    (c, t) =>
                    {
                        if (Throughput > 0)
                        {
                            ThroughputControlChannelModule.CreateAndRegister(c, Throughput);
                        }

                        return Task.CompletedTask;
                    });


            var establishedClientChannelBuilder = new EstablishedClientChannelBuilder(channelBuilder)
                .WithIdentity(Identity)
                .WithAuthentication(GetAuthenticationScheme())
                .WithCompression(Compression)
                .AddEstablishedHandler(SetPresenceAsync)
                .AddEstablishedHandler(SetReceiptAsync)
                .WithEncryption(Encryption);

            if (Instance != null)
            {
                establishedClientChannelBuilder = establishedClientChannelBuilder.WithInstance(Instance);
            }

            var onDemandClientChannelFactory = new OnDemandClientChannelFactory(establishedClientChannelBuilder);

            return new MessagingHubConnection(SendTimeout, MaxConnectionRetries, onDemandClientChannelFactory, ChannelCount);
        }

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (IsGuest(Identifier))
            {
                var guestAuthentication = new GuestAuthentication();
                result = guestAuthentication;
            }

            if (Password != null)
            {
                var plainAuthentication = new PlainAuthentication();
                plainAuthentication.SetToBase64Password(Password);
                result = plainAuthentication;
            }

            if (AccessKey != null)
            {
                var keyAuthentication = new KeyAuthentication { Key = AccessKey };
                result = keyAuthentication;
            }

            if (result == null)
                throw new InvalidOperationException($"A password or accessKey should be defined. Please use the '{nameof(UsingPassword)}' or '{nameof(UsingAccessKey)}' methods for that.");

            return result;
        }

        private async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsGuest(clientChannel.LocalNode.Name))
                await clientChannel.SetResourceAsync(
                        LimeUri.Parse(UriTemplates.PRESENCE),
                        new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule, RoundRobin = true },
                        cancellationToken)
                        .ConfigureAwait(false);
        }

        private static async Task SetReceiptAsync(IClientChannel clientChannel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsGuest(clientChannel.LocalNode.Name))
                await clientChannel.SetResourceAsync(
                        LimeUri.Parse(UriTemplates.RECEIPT),
                        new Receipt { Events = new[] { Event.Accepted, Event.Dispatched, Event.Received, Event.Consumed, Event.Failed } },
                        cancellationToken)
                        .ConfigureAwait(false);
        }

        private static bool IsGuest(string name)
        {
            Guid g;
            return Guid.TryParse(name, out g);
        }
    }
}
