using Lime.Protocol;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClientBuilder
    {
        private string _login;
        private string _password;
        private string _accessKey;

        public const string DEFAULT_DOMAIN = "msging.net";
        private string _domain;
        private string _hostName;
        private Identity _identity => Identity.Parse($"{_login}@{_domain}");
        private Uri _endPoint => new Uri($"net.tcp://{_hostName}:55321");

        public MessagingHubClientBuilder()
        {
            _hostName = DEFAULT_DOMAIN;
            _domain = DEFAULT_DOMAIN;
        }

        public MessagingHubClientBuilder UsingAccount(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException("login");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

            _login = login;
            _password = password;

            return this;
        }

        public MessagingHubClientBuilder UsingAccessKey(string login, string accessKey)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException("login");
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException("accessKey");

            _login = login;
            _accessKey = accessKey;

            return this;
        }

        public MessagingHubClientBuilder UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException("hostName");

            _hostName = hostName;
            return this;
        }

        public MessagingHubClientBuilder UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException("domain");

            _domain = domain;
            return this;
        }

        public MessageHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            return new MessageHubSenderBuilder(_identity, GetAuthenticationScheme(), _endPoint)
                 .AddMessageReceiver(messageReceiver, forMimeType);
        }

        public MessageHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            return new MessageHubSenderBuilder(_identity, GetAuthenticationScheme(), _endPoint)
                .AddMessageReceiver(receiverFactory, forMimeType);
        }

        public MessageHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            return new MessageHubSenderBuilder(_identity, GetAuthenticationScheme(), _endPoint)
                .AddNotificationReceiver(notificationReceiver, forEventType);
        }

        public MessageHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            return new MessageHubSenderBuilder(_identity, GetAuthenticationScheme(), _endPoint)
                .AddNotificationReceiver(receiverFactory, forEventType);
        }

        public IMessagingHubClient Build()
        {
            return new MessagingHubClient(_identity, GetAuthenticationScheme(), _endPoint);
        }

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

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
