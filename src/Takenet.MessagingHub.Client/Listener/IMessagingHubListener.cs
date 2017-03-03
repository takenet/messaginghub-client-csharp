using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Listen to envelopes sent to the account you are connected as
    /// </summary>
    public interface IMessagingHubListener : IStartable, IStoppable
    {
        /// <summary>
        /// Indicates whether or not the listener is active
        /// </summary>
        bool Listening { get; }

        /// <summary>
        /// Add a message receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="messageReceiver">The message receiver that will be invoked when a message that satisfy the given criteria is received</param>
        /// <param name="messageFilter">The criteria to filter the messages</param>
        /// <param name="priority">The priority of the receiver related to the others. Lower values have higher priority. This value can be repeated among receivers. In this cases, the receivers are evaluated in parallel.</param>
        void AddMessageReceiver(IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter, int priority = 0);

        /// <summary>
        /// Add a notification receiver for messages that satisfy the given filter criteria
        /// </summary>
        /// <param name="notificationReceiver">The notification receiver that will be invoked when a notification that satisfy the given criteria is received</param>
        /// <param name="notificationFilter">The criteria to filter the notifications</param>
        /// <param name="priority">The priority of the receiver related to the others. Lower values have higher priority. This value can be repeated among receivers. In this cases, the receivers are evaluated in parallel.</param>
        void AddNotificationReceiver(INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter, int priority = 0);

        /// <summary>
        /// Add a command receiver that satifies the given predicate
        /// </summary>
        /// <param name="commandReceiver"></param>
        /// <param name="commandFilter"></param>
        /// <param name="priority"></param>
        void AddCommandReceiver(ICommandReceiver commandReceiver, Func<Command, Task<bool>> commandFilter, int priority = 0);
    }
}