using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    public interface IEnvelopeListener : IMessagingHubClient
    { 
        ///// <summary>
        ///// Add a message receiver listener to handle received messages.
        ///// </summary>
        ///// <param name="messageReceiver">Listener</param>
        ///// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        ///// <returns></returns>
        void AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null);

        ///// <summary>
        ///// Add a message receiver listener to handle received messages.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        ///// <returns></returns>
        void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null);

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="notificationReceiver">Listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        void AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null);

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = null);
    }
}
