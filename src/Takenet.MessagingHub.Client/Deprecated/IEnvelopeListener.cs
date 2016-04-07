using System;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Deprecated.Receivers;

namespace Takenet.MessagingHub.Client.Deprecated
{
    [Obsolete]
    public interface IEnvelopeListener
    { 
        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The message predicate used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered.</param>
        void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> predicate);

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Predicate<Notification> predicate);
    }
}
