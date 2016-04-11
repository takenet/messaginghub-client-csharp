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
        public IMessagingHubSender Sender { get; }

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
        /// Add a message receiver for the given mime type
        /// </summary>
        /// <param name="messageReceiver">The message receiver that will be invoked when a message of the given mime type is received</param>
        /// <param name="forMimeType">The mime type used to filter the received messages</param>
        public void AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, m => forMimeType == null || Equals(m.Type, forMimeType));
        }

        /// <summary>
        /// Add a notification receiver for the given event type
        /// </summary>
        /// <param name="notificationReceiver">The notification receiver that will be invoked when a notification of the given event type is received</param>
        /// <param name="forEventType">The event type used to filter the received notifications</param>
        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, n => forEventType == null || n.Event == forEventType);
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
        /// Add a message receiver for the given mime type
        /// </summary>
        /// <param name="onMessageReceived">A callback method that will be invoked when a message of the given mime type is received</param>
        /// <param name="forMimeType">The mime type used to filter the received messages</param>
        public void AddMessageReceiver(Action<Message, CancellationToken> onMessageReceived, MediaType forMimeType = null)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => new LambdaMessageReceiver(onMessageReceived), m => forMimeType == null || Equals(m.Type, forMimeType));
        }

        /// <summary>
        /// Add a notification receiver for the given event type
        /// </summary>
        /// <param name="onNotificationReceived">A callback method that will be invoked when a notification of the given event type is received</param>
        /// <param name="forEventType">The event type used to filter the received notifications</param>
        public void AddNotificationReceiver(Action<Notification, CancellationToken> onNotificationReceived, Event? forEventType = null)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => new LambdaNotificationReceiver(onNotificationReceived), n => forEventType == null || n.Event == forEventType);
        }

        /// <summary>
        /// Add a message receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="onMessageReceived">A callback method that will be invoked when a message that satisfy the given criteria is received</param>
        /// <param name="messageFilter">The criteria to filter the messages</param>
        public void AddMessageReceiver(Action<Message, CancellationToken> onMessageReceived, Predicate<Message> messageFilter)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => new LambdaMessageReceiver(onMessageReceived), messageFilter);
        }

        /// <summary>
        /// Add a notification receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="onNotificationReceived">A callback method that will be invoked when a notification that satisfy the given criteria is received</param>
        /// <param name="notificationFilter">The criteria to filter the notifications</param>
        public void AddNotificationReceiver(Action<Notification, CancellationToken> onNotificationReceived, Predicate<Notification> notificationFilter)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => new LambdaNotificationReceiver(onNotificationReceived), notificationFilter);
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
