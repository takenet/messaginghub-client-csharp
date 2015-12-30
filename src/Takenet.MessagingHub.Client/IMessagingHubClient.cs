using Lime.Protocol;
using System;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Allow a client application to connect, send an receive messages, commands and notifications in the Messaging Hub.
    /// <a src="http://messaginghub.io" />
    /// </summary>
    public interface IMessagingHubClient : ICommandSender, IMessageSender, INotificationSender
    {
        /// <summary>
        /// Indicates if the client is already started.
        /// </summary>
        bool Started { get; }

        /// <summary>
        /// Configure the client to authenticate with Messaging Hub with a login and password.
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <returns>The client instance configured to authenticate as requested</returns>
        /// <remarks>If both <see cref="UsingAccount"/> and <see cref="UsingAccessKey"/> are provided, the access key has precedence</remarks>
        MessagingHubClient UsingAccount(string login, string password);

        /// <summary>
        /// Configure the client to authenticate with Messaging Hub with a login and access key.
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="key">Access key</param>
        /// <returns>The client instance configured to authenticate as requested</returns>
        /// <remarks>If both <see cref="UsingAccount"/> and <see cref="UsingAccessKey"/> are provided, the access key has precedence</remarks>
        MessagingHubClient UsingAccessKey(string login, string key);

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="messageReceiver">Listener</param>
        /// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        /// <returns></returns>
        MessagingHubClient AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null);

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        /// <returns></returns>
        MessagingHubClient AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null);

        /// <summary>
        /// Add a notification receiver listener to handle received notifications.
        /// </summary>
        /// <param name="notificationReceiver">Listener</param>
        /// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        /// <returns></returns>
        MessagingHubClient AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null);

        /// <summary>
        /// Add a notification receiver listener to handle received notifications.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        /// <returns></returns>
        MessagingHubClient AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null);

        /// <summary>
        /// Connect and receives messages from the server.
        /// </summary>
        /// <returns>Task representing the running state of the client (when this tasks finishes, the connection has been terminated)</returns>
        Task StartAsync();

        /// <summary>
        /// Close connection and stop to receive messages from the server.
        /// </summary>        
        Task StopAsync();
    }
}
