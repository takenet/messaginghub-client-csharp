using System;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Deprecated.Receivers;

namespace Takenet.MessagingHub.Client.Deprecated
{
    [Obsolete]
    public class MessagingHubSenderBuilder
    {
        private readonly MessagingHubClientBuilder _clientBuilder;

        internal IEnvelopeSender EnvelopeSender => _clientBuilder.MessagingHubClient;

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        public MessagingHubSenderBuilder(MessagingHubClientBuilder clientBuilder)
        {
            _clientBuilder = clientBuilder;
            EnvelopeRegistrar = new EnvelopeListenerRegistrar();
        }

        public MessagingHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(messageReceiver, forMimeType);
            return this;
        }

        public MessagingHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(receiverFactory, forMimeType);
            return this;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(notificationReceiver, forEventType);
            return this;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(receiverFactory, forEventType);
            return this;
        }


        public MessagingHubSenderBuilder AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messagePredicate)
        {
            return AddMessageReceiver(() => messageReceiver, messagePredicate);            
        }

        public MessagingHubSenderBuilder AddMessageReceiver(Func<IMessageReceiver> messageReceiver, Predicate<Message> messagePredicate)
        {
            EnvelopeRegistrar.AddMessageReceiver(messageReceiver, messagePredicate);
            return this;
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationPredicate)
        {
            return AddNotificationReceiver(() => notificationReceiver, notificationPredicate);
        }

        public MessagingHubSenderBuilder AddNotificationReceiver(Func<INotificationReceiver> notificationReceiver, Predicate<Notification> notificationPredicate)
        {
            EnvelopeRegistrar.AddNotificationReceiver(notificationReceiver, notificationPredicate);
            return this;
        }

        public IMessagingHubSender Build() => _clientBuilder.Build();
    }
}
