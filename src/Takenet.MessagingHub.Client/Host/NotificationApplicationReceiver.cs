using System;
using System.Collections.Generic;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Host
{
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