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
    /// <summary>
    /// Builds a connection with the Messaging Hub
    /// </summary>
    public class MessagingHubConnectionBuilder
    {
        public const string DEFAULT_DOMAIN = "msging.net";
        private string _login;
        private string _password;
        private string _accessKey;
        private TimeSpan _sendTimeout;
        private int _maxConnectionRetries;
        private string _domain;
        private string _hostName;
        private SessionCompression _sessionCompression;
        private SessionEncryption _sessionEncryption;
        private IOnDemandClientChannelFactory _onDemandClientChannelFactory;
        private IEstablishedClientChannelBuilder _establishedClientChannelBuilder;

        private Identity _identity => Identity.Parse($"{_login}@{_domain}");

        private Uri _endPoint => new Uri($"net.tcp://{_hostName}:55321");

        /// <summary>
        /// Instantiate a connection builder using the default parameters
        /// </summary>
        public MessagingHubConnectionBuilder()
        {
            _hostName = DEFAULT_DOMAIN;
            _domain = DEFAULT_DOMAIN;
            _sendTimeout = TimeSpan.FromSeconds(20);
            _maxConnectionRetries = 3;
            _sessionCompression = SessionCompression.None;
            _sessionEncryption = SessionEncryption.TLS;
        }

        /// <summary>
        /// Inform an account to be used to connect to the Messaging Hub
        /// </summary>
        /// <param name="account">Account identifier</param>
        /// <param name="password">Account password</param>
        /// <returns>The same builder instance, configured to use the given account</returns>
        public MessagingHubConnectionBuilder UsingAccount(string account, string password)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            _login = account;
            _password = password;

            return this;
        }

        /// <summary>
        /// Use the guest account to connect to the Messaging Hub
        /// </summary>
        /// <returns>The same builder instance, configured to use the guest account</returns>
        public MessagingHubConnectionBuilder UsingGuest()
        {
            _login = Guid.NewGuid().ToString();
            return this;
        }

        /// <summary>
        /// Inform an account to be used to connect to the Messaging Hub
        /// </summary>
        /// <param name="account">Account identifier</param>
        /// <param name="accessKey">Account access key</param>
        /// <returns>The same builder instance, configured to use the given account</returns>
        public MessagingHubConnectionBuilder UsingAccessKey(string account, string accessKey)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));

            _login = account;
            _accessKey = accessKey;

            return this;
        }

        /// <summary>
        /// Overrides the default host name with the given one. It can be used to connect to an alternative instance of the Messaging Hub or another Lime endpoint
        /// </summary>
        /// <param name="hostName">The address of the host, without the protocol prefix and port number sufix</param>
        /// <returns>The same builder instance, configured to use the given host name</returns>
        public MessagingHubConnectionBuilder UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));

            _hostName = hostName;
            return this;
        }

        /// <summary>
        /// Overrides the default domain with the given one
        /// </summary>
        /// <param name="domain">The domain to be used</param>
        /// <returns>The same builder instance, configured to use the given domain</returns>
        public MessagingHubConnectionBuilder UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));

            _domain = domain;
            return this;
        }

        /// <summary>
        /// Overrides the session encryption mode
        /// </summary>
        /// <param name="sessionEncryption">The desired <see cref="SessionEncryption">encryption mode</see></param>
        /// <returns>The same builder instance, configured to use the given encryption mode</returns>
        public MessagingHubConnectionBuilder UsingEncryption(SessionEncryption sessionEncryption)
        {
            _sessionEncryption = sessionEncryption;
            return this;
        }

        /// <summary>
        /// Overrides the session compression mode
        /// </summary>
        /// <param name="sessionCompression">The desired <see cref="SessionCompression">compression mode</see></param>
        /// <returns>The same builder instance, configured to use the given compression mode</returns>
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

        /// <summary>
        /// Overrides the default <see cref="IMessagingHubConnection.SendTimeout">send timeout</see>
        /// </summary>
        /// <param name="timeout">A timespan representing the desired send timeout</param>
        /// <returns>The same builder instance, configured to use the given send timeout</returns>
        public MessagingHubConnectionBuilder WithSendTimeout(TimeSpan timeout)
        {
            _sendTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Overrides the default <see cref="IMessagingHubConnection.MaxConnectionRetries">maximum connection retries</see>
        /// </summary>
        /// <param name="maxConnectionRetries">The maximum number of connection retries. The number must be at least 1 and at most 5</param>
        /// <returns>The same builder instance, configured to use the given maximum connection retries</returns>
        public MessagingHubConnectionBuilder WithMaxConnectionRetries(int maxConnectionRetries)
        {
            if (maxConnectionRetries < 1) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));
            if (maxConnectionRetries > 5) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));

            _maxConnectionRetries = maxConnectionRetries;
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

        /// <summary>
        /// Builds a <see cref="IMessagingHubConnection">connection</see> with the configured parameters
        /// </summary>
        /// <returns>An inactive connection with the Messaging Hub. Call <see cref="IMessagingHubConnection.ConnectAsync"/> to activate it</returns>
        public IMessagingHubConnection Build()
        {
            if (_establishedClientChannelBuilder == null)
            {
                var channelBuilder = ClientChannelBuilder.Create(
                    () => new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer()), _endPoint)
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

            if (_onDemandClientChannelFactory == null)
                _onDemandClientChannelFactory = new OnDemandClientChannelFactory(_establishedClientChannelBuilder);

            var connection = CreateConnection();
            return connection;
        }

        /// <summary>
        /// Creates a connection
        /// </summary>
        protected virtual IMessagingHubConnection CreateConnection()
        {
            var connection = new MessagingHubConnection(_sendTimeout, _maxConnectionRetries, _onDemandClientChannelFactory);
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
