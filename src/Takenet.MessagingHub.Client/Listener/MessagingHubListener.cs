using Lime.Protocol;
using System;
using System.Threading.Tasks;
using Lime.Protocol.Listeners;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class MessagingHubListener : IWorker
    {
        public MessagingHubConnection Connection { get; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        private ChannelListener ChannelListener { get; set; }

        public MessagingHubListener(MessagingHubConnection connection)
        {
            Connection = connection;
            EnvelopeRegistrar = new EnvelopeListenerRegistrar();
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, m => Equals(m.Type, forMimeType));
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, n => n.Event == forEventType);
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, messageFilter);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, notificationFilter);
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
            var sender = new MessagingHubSender(Connection);
            var messageHandler = new MessageReceivedHandler(sender, EnvelopeRegistrar);
            var notificationHandler = new NotificationReceivedHandler(sender, EnvelopeRegistrar);
            var commandHandler = new CommandReceivedHandler(sender, EnvelopeRegistrar);
            ChannelListener = new ChannelListener(
                messageHandler.HandleAsync,
                notificationHandler.HandleAsync,
                commandHandler.HandleAsync);
            ChannelListener.Start(Connection.OnDemandClientChannel);
        }
    }
}
