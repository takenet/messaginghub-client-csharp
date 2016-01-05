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
    public class MessageHubSenderBuilder
    {
        private EnvelopeListener _envelopeListenner;

        public MessageHubSenderBuilder(string login, Authentication authentication, Uri endPoint, string domain)
        {
            _envelopeListenner = new EnvelopeListener(login, authentication, endPoint, domain);
        }
        
        public IMessagingHubSender Build()
        {
            return _envelopeListenner;
        }

        public MessageHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            _envelopeListenner.AddMessageReceiver(messageReceiver, forMimeType);
            return this;
        }

        public MessageHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            _envelopeListenner.AddMessageReceiver(receiverFactory, forMimeType);
            return this;
        }

        public MessageHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            _envelopeListenner.AddNotificationReceiver(notificationReceiver, forEventType);
            return this;
        }

        public MessageHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            _envelopeListenner.AddNotificationReceiver(receiverFactory, forEventType);
            return this;
        }
    }
}
