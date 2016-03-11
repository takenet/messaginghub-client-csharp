using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder
    {
        public const string DEFAULT_DOMAIN = "msging.net";
        private const string GUEST = "guest";

        private readonly MessagingHubSenderBuilder _senderBuilder;

        private string _login;
        private string _password;
        private string _accessKey;
        private TimeSpan _sendTimeout;
        private string _domain;
        private string _hostName;

        private Identity _identity => Identity.Parse($"{_login}@{_domain}");
        private Uri _endPoint => new Uri($"net.tcp://{_hostName}:55321");

        internal MessagingHubClient MessagingHubClient { get; private set; }

        public MessagingHubClientBuilder()
        {
            _hostName = DEFAULT_DOMAIN;
            _domain = DEFAULT_DOMAIN;
            _sendTimeout = TimeSpan.FromSeconds(20);
            _senderBuilder = new MessagingHubSenderBuilder(this);
        }

        public MessagingHubClientBuilder UsingAccount(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            _login = login;
            _password = password;

            return this;
        }

        public MessagingHubClientBuilder UsingGuest()
        {
            _login = GUEST;
            return this;
        }

        public MessagingHubClientBuilder UsingAccessKey(string login, string accessKey)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));

            _login = login;
            _accessKey = accessKey;

            return this;
        }

        public MessagingHubClientBuilder UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));

            _hostName = hostName;
            return this;
        }

        public MessagingHubClientBuilder UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));

            _domain = domain;
            return this;
        }

        public MessagingHubClientBuilder WithSendTimeout(TimeSpan timeout)
        {
            _sendTimeout = timeout;
            return this;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            _senderBuilder.AddMessageReceiver(messageReceiver, forMimeType);
            return _senderBuilder;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            _senderBuilder.AddMessageReceiver(receiverFactory, forMimeType);
            return _senderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            _senderBuilder.AddNotificationReceiver(notificationReceiver, forEventType);
            return _senderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            _senderBuilder.AddNotificationReceiver(receiverFactory, forEventType);
            return _senderBuilder;
        }

        public IMessagingHubClient Build()
        {
            MessagingHubClient = new MessagingHubClient(_identity, GetAuthenticationScheme(), _endPoint, _sendTimeout, _senderBuilder.EnvelopeRegistrar);
            return MessagingHubClient;
        }

        internal MessagingHubSenderBuilder AsMessagingSenderBuilder() => _senderBuilder;

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (_login == GUEST)
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
    }
}
