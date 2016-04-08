using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Util;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public sealed class MessagingHubListener : IWorker
    {
        private MessagingHubConnection Connection { get; }

        private IMessagingHubSender Sender { get; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        private ChannelListener ChannelListener { get; set; }

        public MessagingHubListener(MessagingHubConnection connection)
        {
            Connection = connection;
            Sender = new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar();
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, m => forMimeType == null || Equals(m.Type, forMimeType));
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, n => forEventType == null || n.Event == forEventType);
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, messageFilter);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, notificationFilter);
        }

        public void AddMessageReceiver(Action<Message, CancellationToken> onMessageReceived, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => new LambdaMessageReceiver(onMessageReceived), m => forMimeType == null || Equals(m.Type, forMimeType));
        }

        public void AddNotificationReceiver(Action<Notification, CancellationToken> onNotificationReceived, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => new LambdaNotificationReceiver(onNotificationReceived), n => forEventType == null || n.Event == forEventType);
        }

        public void AddMessageReceiver(Action<Message, CancellationToken> onMessageReceived, Predicate<Message> messageFilter)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => new LambdaMessageReceiver(onMessageReceived), messageFilter);
        }

        public void AddNotificationReceiver(Action<Notification, CancellationToken> onNotificationReceived, Predicate<Notification> notificationFilter)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => new LambdaNotificationReceiver(onNotificationReceived), notificationFilter);
        }

        public Task StartAsync()
        {
            StartEnvelopeListeners();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            ChannelListener?.Stop();
            return Task.CompletedTask;
        }

        private void StartEnvelopeListeners()
        {
            var messageHandler = new MessageReceivedHandler(Sender, EnvelopeRegistrar);
            var notificationHandler = new NotificationReceivedHandler(Sender, EnvelopeRegistrar);
            ChannelListener = new ChannelListener(
                messageHandler.HandleAsync,
                notificationHandler.HandleAsync,
                c => TaskUtil.TrueCompletedTask);
            ChannelListener.Start(Connection.OnDemandClientChannel);
        }
    }
}
