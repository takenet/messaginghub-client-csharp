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
    /// <summary>
    /// Listen to envelopes sent to the account you are connected as
    /// </summary>
    public sealed class MessagingHubListener : IWorker
    {
        /// <summary>
        /// The connection used to listen through
        /// </summary>
        public MessagingHubConnection Connection { get; }

        /// <summary>
        /// A sender used to send responses to the envelopes received
        /// </summary>
        public MessagingHubSender Sender { get; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        private ChannelListener ChannelListener { get; set; }
        public bool Listening { get; private set; }

        /// <summary>
        /// Instantiate a listener using the given connection
        /// </summary>
        /// <param name="connection"></param>
        public MessagingHubListener(MessagingHubConnection connection)
        {
            Connection = connection;
            Sender = new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar(this);
        }

        /// <summary>
        /// Add a message receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="messageReceiver">The message receiver that will be invoked when a message that satisfy the given criteria is received</param>
        /// <param name="messageFilter">The criteria to filter the messages</param>
        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, messageFilter);
        }

        /// <summary>
        /// Add a notification receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="notificationReceiver">The notification receiver that will be invoked when a notification that satisfy the given criteria is received</param>
        /// <param name="notificationFilter">The criteria to filter the notifications</param>
        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, notificationFilter);
        }

        /// <summary>
        /// Starts to listen to the registered receivers. No receivers can be added after the listener is started
        /// </summary>
        public Task StartAsync()
        {
            StartEnvelopeListeners();
            Listening = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops listening to the connection
        /// </summary>
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
