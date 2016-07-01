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
    internal sealed class MessagingHubListener : IMessagingHubListener
    {
        private IMessagingHubConnection Connection { get; }

        private IMessagingHubSender Sender { get; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        private ChannelListener ChannelListener { get; set; }

        public bool Listening { get; private set; }

        public MessagingHubListener(IMessagingHubConnection connection, IMessagingHubSender sender = null)
        {
            Connection = connection;
            Sender = sender ?? new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar(this);
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter, int priority = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, messageFilter, priority, cancellationToken);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter, int priority = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, notificationFilter, priority, cancellationToken);
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            StartEnvelopeListeners();
            Listening = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
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
