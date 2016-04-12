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
    public sealed class MessagingHubListener : IMessagingHubListener
    {
        public IMessagingHubConnection Connection { get; }

        public IMessagingHubSender Sender { get; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        private ChannelListener ChannelListener { get; set; }

        public bool Listening { get; private set; }

        public MessagingHubListener(IMessagingHubConnection connection, IMessagingHubSender sender = null)
        {
            Connection = connection;
            Sender = sender ?? new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar(this);
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
            Listening = true;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            StopEnvelopeListeners();
            Listening = false;
            return Task.CompletedTask;
        }

        private void StopEnvelopeListeners()
        {
            ChannelListener?.Stop();
            ChannelListener?.Dispose();
            ChannelListener = null;
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
