using System;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    public static class EnvelopeListenerExtensions
    {
        ///// <summary>
        ///// Add a message receiver listener to handle received messages.
        ///// </summary>
        ///// <param name="messageReceiver">Listener</param>
        ///// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        ///// <returns></returns>
        public static void AddMessageReceiver(this IEnvelopeListener envelopeListener, IMessageReceiver messageReceiver,
            MediaType forMimeType = null)
        {
            AddMessageReceiver(envelopeListener, () => messageReceiver, forMimeType);            
        }

        ///// <summary>
        ///// Add a message receiver listener to handle received messages.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        ///// <returns></returns>
        public static void AddMessageReceiver(this IEnvelopeListener envelopeListener,
            Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {                        
            if (forMimeType == null ||
                forMimeType.Equals(MediaTypes.Any))
            {
                envelopeListener.AddMessageReceiver(receiverFactory, m => true);                
            }
            else
            {
                envelopeListener.AddMessageReceiver(receiverFactory, m => m.Type.Equals(forMimeType));
            }
        }

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="notificationReceiver">Listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        public static void AddNotificationReceiver(this IEnvelopeListener envelopeListener,
            INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            AddNotificationReceiver(envelopeListener, () => notificationReceiver, forEventType);
        }

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        public static void AddNotificationReceiver(this IEnvelopeListener envelopeListener,
            Func<INotificationReceiver> receiverFactory, Event? forEventType = null)
        {
            if (forEventType == null)
            {
                envelopeListener.AddNotificationReceiver(receiverFactory, n => true);                
            }
            else
            {
                envelopeListener.AddNotificationReceiver(receiverFactory, n => n.Event.Equals(forEventType));
            }
        }

    }
}