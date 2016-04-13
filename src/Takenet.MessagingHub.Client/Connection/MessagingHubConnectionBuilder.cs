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
    public class MessagingHubConnectionBuilder : MessagingHubConnectionConfigurator<MessagingHubConnectionBuilder>
    {

        /// <summary>
        /// Builds a <see cref="IMessagingHubConnection">connection</see> with the configured parameters
        /// </summary>
        /// <returns>An inactive connection with the Messaging Hub. Call <see cref="IMessagingHubConnection.ConnectAsync"/> to activate it</returns>
        public IMessagingHubConnection Build()
        {
            var channelBuilder = ClientChannelBuilder.Create(
                () => new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer()), EndPoint)
                                 .WithSendTimeout(SendTimeout)
                                 .WithBuffersLimit(100)
                                 .AddMessageModule(c => new NotifyReceiptChannelModule(c))
                                 .AddCommandModule(c => new ReplyPingChannelModule(c));

            var establishedClientChannelBuilder = new EstablishedClientChannelBuilder(channelBuilder)
                .WithIdentity(Identity)
                .WithAuthentication(GetAuthenticationScheme())
                .WithCompression(Compression)
                .AddEstablishedHandler(SetPresenceAsync)
                .AddEstablishedHandler(SetReceiptAsync)
                .WithEncryption(Encryption);


            var onDemandClientChannelFactory = new OnDemandClientChannelFactory(establishedClientChannelBuilder);

            return new MessagingHubConnection(SendTimeout, MaxConnectionRetries, onDemandClientChannelFactory);
        }

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (IsGuest(Account))
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
                throw new InvalidOperationException($"A password or accessKey should be defined. Please use the '{nameof(UsingAccount)}' or '{nameof(UsingAccessKey)}' methods for that.");

            return result;
        }

        private static async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsGuest(clientChannel.LocalNode.Name))
                await clientChannel.SetResourceAsync(
                        LimeUri.Parse(UriTemplates.PRESENCE),
                        new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity, RoundRobin = true },
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
