using System;
using System.Collections.Generic;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Host
{
    public class ApplicationNotificationReceiver
    {
        /// <summary>
        /// Gets or sets the receiver .NET type. It must implement <see cref="INotificationReceiver"/> or <see cref="IFactory{INotificationReceiver}"/>.
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{TKey,TValue}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the event. 
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public Event? EventType { get; set; }

    }
}