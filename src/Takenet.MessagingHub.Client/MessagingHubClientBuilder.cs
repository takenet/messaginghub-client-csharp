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
        private Uri _endPoint;
        private string _domain;

        public MessagingHubClientBuilder()
        {
            _endPoint = BuildEndPoint(DEFAULT_DOMAIN);
            _domain = DEFAULT_DOMAIN;
        }

        public MessagingHubClientBuilder UsingAccount(string login, string password)
        {
            _login = login;
            _password = password;

            return this;
        }

        public MessagingHubClientBuilder UsingAccessKey(string login, string accessKey)
        {
            _login = login;
            _accessKey = accessKey;

            return this;
        }

        public MessagingHubClientBuilder UsingHostName(string hostName)
        {
            _endPoint = BuildEndPoint(hostName);
            return this;
        }

        public MessagingHubClientBuilder UsingDomain(string domain)
        {
            _domain = domain;
            return this;
        }

        public MessageHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            return new MessageHubSenderBuilder(_login, GetAuthenticationScheme(), _endPoint, _domain)
                 .AddMessageReceiver(messageReceiver, forMimeType);
        }

        public MessageHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            return new MessageHubSenderBuilder(_login, GetAuthenticationScheme(), _endPoint, _domain)
                .AddMessageReceiver(receiverFactory, forMimeType);
        }

        public MessageHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            return new MessageHubSenderBuilder(_login, GetAuthenticationScheme(), _endPoint, _domain)
                .AddNotificationReceiver(notificationReceiver, forEventType);
        }

        public MessageHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            return new MessageHubSenderBuilder(_login, GetAuthenticationScheme(), _endPoint, _domain)
                .AddNotificationReceiver(receiverFactory, forEventType);
        }

        public IMessagingHubClient Build()
        {
            return new MessagingHubClient(_login, GetAuthenticationScheme(), _endPoint, _domain);
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

        private static Uri BuildEndPoint(string hostName)
        {
            return new Uri($"net.tcp://{hostName}:55321");
        }
    }
}
