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
        internal readonly EnvelopeListener EnvelopeListener;

        public MessageHubSenderBuilder(string login, Authentication authentication, Uri endPoint, string domain)
        {
            EnvelopeListener = new EnvelopeListener(login, authentication, endPoint, domain);
        }
        
        public MessageHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            EnvelopeListener.AddMessageReceiver(messageReceiver, forMimeType);
            return this;
        }

        public MessageHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            EnvelopeListener.AddMessageReceiver(receiverFactory, forMimeType);
            return this;
        }

        public MessageHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            EnvelopeListener.AddNotificationReceiver(notificationReceiver, forEventType);
            return this;
        }

        public MessageHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            EnvelopeListener.AddNotificationReceiver(receiverFactory, forEventType);
            return this;
        }

        public IMessagingHubSender Build()
        {
            return EnvelopeListener;
        }
    }
}
