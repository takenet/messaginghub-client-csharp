using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Extension methods to simplify the usage of the <see cref="MessagingHubListener"/>
    /// </summary>
    public static class MessagingHubListenerExtensions
    {
        /// <summary>
        /// Add a message receiver for the given mime type
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="messageReceiver">The message receiver that will be invoked when a message of the given mime type is received</param>
        /// <param name="forMimeType">The mime type used to filter the received messages</param>
        public static void AddMessageReceiver(this IMessagingHubListener listener, IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            listener.AddMessageReceiver(messageReceiver, m => Task.FromResult(forMimeType == null || Equals(m.Type, forMimeType)));
        }

        /// <summary>
        /// Add a notification receiver for the given event type
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="notificationReceiver">The notification receiver that will be invoked when a notification of the given event type is received</param>
        /// <param name="forEventType">The event type used to filter the received notifications</param>
        public static void AddNotificationReceiver(this IMessagingHubListener listener, INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            listener.AddNotificationReceiver(notificationReceiver, n => Task.FromResult(forEventType == null || n.Event == forEventType));
        }

        /// <summary>
        /// Add a command receiver that will accept all commands
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="commandReceiver"></param>
        public static void AddCommandReceiver(this IMessagingHubListener listener, ICommandReceiver commandReceiver)
        {
            listener.AddCommandReceiver(commandReceiver, c => Task.FromResult(true));
        }

        /// <summary>
        /// Add a message receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="messageReceiver">The message receiver that will be invoked when a message that satisfy the given criteria is received</param>
        /// <param name="messageFilter">The criteria to filter the messages</param>
        public static void AddMessageReceiver(this IMessagingHubListener listener, IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter)
        {
            listener.AddMessageReceiver(messageReceiver, messageFilter);
        }

        /// <summary>
        /// Add a command receiver for commands that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="commandReceiver"></param>
        /// <param name="commandFilter"></param>
        public static void AddCommandReceiver(this IMessagingHubListener listener, ICommandReceiver commandReceiver, Func<Message, Task<bool>> commandFilter)
        {
            listener.AddCommandReceiver(commandReceiver, commandFilter);
        }

        /// <summary>
        /// Add a notification receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="notificationReceiver">The notification receiver that will be invoked when a notification that satisfy the given criteria is received</param>
        /// <param name="notificationFilter">The criteria to filter the notifications</param>
        public static void AddNotificationReceiver(this IMessagingHubListener listener, INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter)
        {
            listener.AddNotificationReceiver(notificationReceiver, notificationFilter);
        }

        /// <summary>
        /// Add a message receiver for the given mime type
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="onMessageReceived">A callback method that will be invoked when a message of the given mime type is received</param>
        /// <param name="forMimeType">The mime type used to filter the received messages</param>
        public static void AddMessageReceiver(this IMessagingHubListener listener, Func<Message, CancellationToken, Task> onMessageReceived, MediaType forMimeType = null)
        {
            listener.AddMessageReceiver(new LambdaMessageReceiver(onMessageReceived), m => Task.FromResult(forMimeType == null || Equals(m.Type, forMimeType)));
        }
        
        /// <summary>
        /// Add a notification receiver for the given event type
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="onNotificationReceived">A callback method that will be invoked when a notification of the given event type is received</param>
        /// <param name="forEventType">The event type used to filter the received notifications</param>
        public static void AddNotificationReceiver(this IMessagingHubListener listener, Func<Notification, CancellationToken, Task> onNotificationReceived, Event? forEventType = null)
        {
            listener.AddNotificationReceiver(new LambdaNotificationReceiver(onNotificationReceived), n => Task.FromResult(forEventType == null || n.Event == forEventType));
        }

        /// <summary>
        /// Add a message receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="onMessageReceived">A callback method that will be invoked when a message that satisfy the given criteria is received</param>
        /// <param name="messageFilter">The criteria to filter the messages</param>
        public static void AddMessageReceiver(this IMessagingHubListener listener, Func<Message, CancellationToken, Task> onMessageReceived, Func<Message, Task<bool>> messageFilter)
        {
            listener.AddMessageReceiver(new LambdaMessageReceiver(onMessageReceived), messageFilter);
        }

        /// <summary>
        /// Add a notification receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener">The listener to add the receivers to</param>
        /// <param name="onNotificationReceived">A callback method that will be invoked when a notification that satisfy the given criteria is received</param>
        /// <param name="notificationFilter">The criteria to filter the notifications</param>
        public static void AddNotificationReceiver(this IMessagingHubListener listener, Func<Notification, CancellationToken, Task> onNotificationReceived, Func<Notification, Task<bool>> notificationFilter)
        {
            listener.AddNotificationReceiver(new LambdaNotificationReceiver(onNotificationReceived), notificationFilter);
        }

        /// <summary>
        /// Add a command receiver for commands that satisfy the given filter criteria
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="onCommandReceived"></param>
        /// <param name="commandFilter"></param>
        public static void AddCommandReceiver(this IMessagingHubListener listener, Func<Command, CancellationToken, Task> onCommandReceived, Func<Command, Task<bool>> commandFilter)
        {
            listener.AddCommandReceiver(new LambdaCommandReceiver(onCommandReceived), commandFilter);
        }
    }
}
