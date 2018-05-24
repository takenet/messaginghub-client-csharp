using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Network.Modules;
using Lime.Protocol.Security;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder
    {
        private readonly ITransportFactory _transportFactory;

        public MessagingHubClientBuilder()
            : this(new TcpTransportFactory())
        {

        }

        public MessagingHubClientBuilder(ITransportFactory transportFactory)
        {
            _transportFactory = transportFactory ?? throw new ArgumentNullException(nameof(transportFactory));

            // Initialize the defaults
            Domain = Constants.DEFAULT_DOMAIN;
            Scheme = Constants.DEFAULT_SCHEME;
            HostName = Constants.DEFAULT_HOST_NAME;
            Port = Constants.DEFAULT_PORT;
            SendTimeout = TimeSpan.FromSeconds(60);
            MaxConnectionRetries = 3;
            Compression = SessionCompression.None;
            Encryption = SessionEncryption.TLS;
            RoutingRule = RoutingRule.Identity;
            RoundRobin = true;
            AutoNotify = true;
            ChannelCount = 1;
            ReceiptEvents = new[] { Event.Accepted, Event.Dispatched, Event.Received, Event.Consumed, Event.Failed };
        }

        public string Identifier { get; private set; }

        public string Instance { get; private set; }

        public string Password { get; private set; }

        public string AccessKey { get; private set; }

        public TimeSpan SendTimeout { get; private set; }

        public int MaxConnectionRetries { get; private set; }

        public string Domain { get; private set; }

        public string Scheme { get; private set; }

        public string HostName { get; private set; }

        public int Port { get; private set; }

        public SessionCompression Compression { get; private set; }

        public SessionEncryption Encryption { get; private set; }

        public RoutingRule RoutingRule { get; private set; }

        public bool RoundRobin { get; private set; }

        public int Throughput { get; private set; }

        public bool AutoNotify { get; private set; }

        public int ChannelCount { get; private set; }

        public Identity Identity => new Identity(Identifier, Domain);

        public Uri EndPoint => new Uri($"{Scheme}://{HostName}:{Port}");

        public Event[] ReceiptEvents { get; private set; }

        public MessagingHubClientBuilder UsingPassword(string identifier, string password)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            Identifier = identifier;
            Password = password;

            return this;
        }

        public MessagingHubClientBuilder UsingGuest()
        {
            Identifier = Guid.NewGuid().ToString();
            return this;
        }

        public MessagingHubClientBuilder UsingAccessKey(string identifier, string accessKey)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));
            Identifier = identifier;
            AccessKey = accessKey;
            return this;
        }

        public MessagingHubClientBuilder UsingInstance(string instance)
        {
            Instance = instance;
            return this;
        }

        public MessagingHubClientBuilder UsingRoutingRule(RoutingRule routingRule)
        {
            RoutingRule = routingRule;
            return this;
        }

        public MessagingHubClientBuilder UsingRoundRobin(bool roundRobin)
        {
            RoundRobin = roundRobin;
            return this;
        }

        public MessagingHubClientBuilder UsingScheme(string scheme)
        {
            if (string.IsNullOrEmpty(scheme)) throw new ArgumentNullException(nameof(scheme));
            Scheme = scheme;
            return this;
        }

        public MessagingHubClientBuilder UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));
            HostName = hostName;
            return this;
        }

        public MessagingHubClientBuilder UsingPort(int port)
        {
            if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));
            Port = port;
            return this;
        }

        public MessagingHubClientBuilder UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));
            Domain = domain;
            return this;
        }

        public MessagingHubClientBuilder UsingEncryption(SessionEncryption sessionEncryption)
        {
            Encryption = sessionEncryption;
            return this;
        }

        public MessagingHubClientBuilder UsingCompression(SessionCompression sessionCompression)
        {
            Compression = sessionCompression;
            return this;
        }

        public MessagingHubClientBuilder WithChannelCount(int channelCount)
        {
            if (channelCount <= 0) throw new ArgumentOutOfRangeException(nameof(channelCount));
            ChannelCount = channelCount;
            return this;
        }

        public MessagingHubClientBuilder WithSendTimeout(TimeSpan timeout)
        {
            if (timeout == default(TimeSpan)) throw new ArgumentOutOfRangeException(nameof(timeout));
            SendTimeout = timeout;
            return this;
        }

        public MessagingHubClientBuilder WithAutoNotify(bool enabled)
        {
            AutoNotify = enabled;
            return this;
        }

        public MessagingHubClientBuilder WithReceiptEvents(Event[] events)
        {
            ReceiptEvents = events ?? throw new ArgumentNullException(nameof(events));
            return this;
        }

        public MessagingHubClientBuilder WithMaxConnectionRetries(int maxConnectionRetries)
        {
            if (maxConnectionRetries < 1) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));
            if (maxConnectionRetries > 5) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));

            MaxConnectionRetries = maxConnectionRetries;
            return this;
        }

        public MessagingHubClientBuilder WithThroughput(int throughput)
        {
            Throughput = throughput;
            return this;
        }

        /// <summary>
        /// Builds an <see cref="IBlipClient" /> with the configured parameters
        /// </summary>
        public IMessagingHubClient Build()
        {
            var channelBuilder = ClientChannelBuilder
                .Create(() => _transportFactory.Create(EndPoint), EndPoint)
                .WithSendTimeout(SendTimeout)
                .WithEnvelopeBufferSize(100)
                .AddCommandModule(c => new ReplyPingChannelModule(c))
                .AddBuiltHandler(
                    (c, t) =>
                    {
                        if (Throughput > 0) ThroughputControlChannelModule.CreateAndRegister(c, Throughput);
                        return Task.CompletedTask;
                    });

            var establishedClientChannelBuilder = new EstablishedClientChannelBuilder(channelBuilder)
                .WithIdentity(Identity)
                .WithAuthentication(GetAuthenticationScheme())
                .WithCompression(Compression)
                .WithEncryption(Encryption)
                .AddEstablishedHandler(SetPresenceAsync)
                .AddEstablishedHandler(SetReceiptAsync);

            if (Instance != null)
            {
                establishedClientChannelBuilder = establishedClientChannelBuilder.WithInstance(Instance);
            }

            var onDemandClientChannel = CreateOnDemandClientChannel(establishedClientChannelBuilder);
            return new MessagingHubClient(onDemandClientChannel);
        }

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (IsGuest(Identifier))
            {
                result = new GuestAuthentication();
            }

            if (Password != null)
            {
                result = new PlainAuthentication()
                {
                    Password = Password
                };
            }

            if (AccessKey != null)
            {
                result = new KeyAuthentication { Key = AccessKey };
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"A password or accessKey should be defined. Please use the '{nameof(UsingPassword)}' or '{nameof(UsingAccessKey)}' methods for that.");
            }

            return result;
        }

        private async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsGuest(clientChannel.LocalNode.Name)) return;

            await clientChannel.SetResourceAsync(
                    LimeUri.Parse(UriTemplates.PRESENCE),
                    new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule, RoundRobin = RoundRobin },
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task SetReceiptAsync(IClientChannel clientChannel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsGuest(clientChannel.LocalNode.Name)
                || ReceiptEvents.Length == 0) return;

            await clientChannel.SetResourceAsync(
                    LimeUri.Parse(UriTemplates.RECEIPT),
                    new Receipt { Events = ReceiptEvents },
                    cancellationToken)
                    .ConfigureAwait(false);
        }

        private static bool IsGuest(string name) => Guid.TryParse(name, out var _);

        private IOnDemandClientChannel CreateOnDemandClientChannel(IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            IOnDemandClientChannel onDemandClientChannel;

            // Avoid the overhead of the MultiplexerClientChannel for a single connection
            if (ChannelCount == 1)
            {
                onDemandClientChannel = new OnDemandClientChannel(establishedClientChannelBuilder);
            }
            else
            {
                onDemandClientChannel = new MultiplexerClientChannel(establishedClientChannelBuilder, ChannelCount);
            }
            return onDemandClientChannel;
        }
    }
}