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
    public class MessagingHubConnectionBuilder
    {
        public const string DEFAULT_DOMAIN = "msging.net";
        private string _login;
        private string _password;
        private string _accessKey;
        private TimeSpan _sendTimeout;
        private string _domain;
        private string _hostName;
        private SessionCompression _sessionCompression;
        private SessionEncryption _sessionEncryption;
        private IOnDemandClientChannelFactory _onDemandClientChannelFactory;
        private IEstablishedClientChannelBuilder _establishedClientChannelBuilder;

        private Identity _identity => Identity.Parse($"{_login}@{_domain}");

        private Uri _endPoint => new Uri($"net.tcp://{_hostName}:55321");

        public MessagingHubConnectionBuilder()
        {
            _hostName = DEFAULT_DOMAIN;
            _domain = DEFAULT_DOMAIN;
            _sendTimeout = TimeSpan.FromSeconds(20);
            _sessionCompression = SessionCompression.None;
            _sessionEncryption = SessionEncryption.TLS;
        }

        public MessagingHubConnectionBuilder UsingAccount(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            _login = login;
            _password = password;

            return this;
        }

        public MessagingHubConnectionBuilder UsingGuest()
        {
            _login = Guid.NewGuid().ToString();
            return this;
        }

        public MessagingHubConnectionBuilder UsingAccessKey(string login, string accessKey)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));

            _login = login;
            _accessKey = accessKey;

            return this;
        }

        public MessagingHubConnectionBuilder UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));

            _hostName = hostName;
            return this;
        }

        public MessagingHubConnectionBuilder UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));

            _domain = domain;
            return this;
        }

        public MessagingHubConnectionBuilder UsingEncryption(SessionEncryption sessionEncryption)
        {
            _sessionEncryption = sessionEncryption;
            return this;
        }

        public MessagingHubConnectionBuilder UsingCompression(SessionCompression sessionCompression)
        {
            _sessionCompression = sessionCompression;
            return this;
        }

        internal MessagingHubConnectionBuilder UsingOnDemandClientChannelFactory(IOnDemandClientChannelFactory onDemandClientChannelFactory)
        {
            _onDemandClientChannelFactory = onDemandClientChannelFactory;
            return this;
        }

        internal MessagingHubConnectionBuilder UsingEstablishedClientChannelBuilder(IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            _establishedClientChannelBuilder = establishedClientChannelBuilder;
            return this;
        }

        public MessagingHubConnectionBuilder WithSendTimeout(TimeSpan timeout)
        {
            _sendTimeout = timeout;
            return this;
        }

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (IsGuest(_login))
            {
                var guestAuthentication = new GuestAuthentication();
                result = guestAuthentication;
            }

            if (_password != null)
            {
                var plainAuthentication = new PlainAuthentication();
                plainAuthentication.SetToBase64Password(_password);
                result = plainAuthentication;
            }

            if (_accessKey != null)
            {
                var keyAuthentication = new KeyAuthentication { Key = _accessKey };
                result = keyAuthentication;
            }

            if (result == null)
                throw new InvalidOperationException($"A password or accessKey should be defined. Please use the '{nameof(UsingAccount)}' or '{nameof(UsingAccessKey)}' methods for that.");

            return result;
        }

        public IMessagingHubConnection Build()
        {
            if (_onDemandClientChannelFactory == null)
                _onDemandClientChannelFactory = new OnDemandClientChannelFactory();

            if (_establishedClientChannelBuilder == null)
            {
                var channelBuilder = ClientChannelBuilder.Create(() => new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer()), _endPoint)
                                     .WithSendTimeout(_sendTimeout)
                                     .WithBuffersLimit(100)
                                     .AddMessageModule(c => new NotifyReceiptChannelModule(c))
                                     .AddCommandModule(c => new ReplyPingChannelModule(c));

                _establishedClientChannelBuilder = new EstablishedClientChannelBuilder(channelBuilder)
                    .WithIdentity(_identity)
                    .WithAuthentication(GetAuthenticationScheme())
                    .WithCompression(_sessionCompression)
                    .AddEstablishedHandler(SetPresenceAsync)
                    .AddEstablishedHandler(SetReceiptAsync)
                    .WithEncryption(_sessionEncryption);
            }

            var connection = new MessagingHubConnection(_sendTimeout, _onDemandClientChannelFactory, _establishedClientChannelBuilder);
            return connection;
        }

        private static async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken)
        {
            if (!IsGuest(clientChannel.LocalNode.Name))
                await clientChannel.SetResourceAsync(
                        LimeUri.Parse(UriTemplates.PRESENCE),
                        new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity, RoundRobin = true },
                        cancellationToken)
                        .ConfigureAwait(false);
        }

        private static async Task SetReceiptAsync(IClientChannel clientChannel, CancellationToken cancellationToken)
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
