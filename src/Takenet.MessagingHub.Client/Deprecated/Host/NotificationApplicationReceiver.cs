using System;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Host
{
    [Obsolete]
    public class NotificationApplicationReceiver : ApplicationReceiver
    {
        /// <summary>
        /// Gets or sets the type of the event. 
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public Event? EventType { get; set; }
    }
}