using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder
    {
        public const string DEFAULT_DOMAIN = "msging.net";
        private string _login;
        private string _password;
        private string _accessKey;
        private TimeSpan _sendTimeout;
        private string _domain;
        private string _hostName;
        private SessionCompression _sessionCompression = SessionCompression.None;
        private SessionEncryption _sessionEncryption = SessionEncryption.TLS;

        internal readonly MessagingHubSenderBuilder SenderBuilder;

        private Identity _identity => Identity.Parse($"{_login}@{_domain}");
        private Uri _endPoint => new Uri($"net.tcp://{_hostName}:55321");

        internal MessagingHubClient MessagingHubClient { get; private set; }

        public MessagingHubClientBuilder()
        {
            _hostName = DEFAULT_DOMAIN;
            _domain = DEFAULT_DOMAIN;
            _sendTimeout = TimeSpan.FromSeconds(20);
            SenderBuilder = new MessagingHubSenderBuilder(this);
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
            _login = Guid.NewGuid().ToString();
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

        public MessagingHubClientBuilder UsingEncryption(SessionEncryption sessionEncryption)
        {
            _sessionEncryption = sessionEncryption;
            return this;
        }

        public MessagingHubClientBuilder UsingCompression(SessionCompression sessionCompression)
        {
            _sessionCompression = sessionCompression;
            return this;
        }

        public MessagingHubClientBuilder WithSendTimeout(TimeSpan timeout)
        {
            _sendTimeout = timeout;
            return this;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            SenderBuilder.AddMessageReceiver(messageReceiver, forMimeType);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            SenderBuilder.AddMessageReceiver(receiverFactory, forMimeType);
            return SenderBuilder;
        }


        public MessagingHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messagePredicate)
        {
            SenderBuilder.AddMessageReceiver(messageReceiver, messagePredicate);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> messagePredicate)
        {
            SenderBuilder.AddMessageReceiver(receiverFactory, messagePredicate);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            SenderBuilder.AddNotificationReceiver(notificationReceiver, forEventType);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            SenderBuilder.AddNotificationReceiver(receiverFactory, forEventType);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationPredicate)
        {
            SenderBuilder.AddNotificationReceiver(notificationReceiver, notificationPredicate);
            return SenderBuilder;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Predicate<Notification> notificationPredicate)
        {
            SenderBuilder.AddNotificationReceiver(receiverFactory, notificationPredicate);
            return SenderBuilder;
        }

        public IMessagingHubClient Build()
        {
            MessagingHubClient = new MessagingHubClient(_identity, GetAuthenticationScheme(), _endPoint, _sendTimeout, SenderBuilder.EnvelopeRegistrar, _sessionCompression, _sessionEncryption);
            return MessagingHubClient;
        }

        internal MessagingHubSenderBuilder AsMessagingSenderBuilder() => SenderBuilder;

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (MessagingHubClient.IsGuest(_login))
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
