using System;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Deprecated.Host
{
    [Obsolete]
    public class ApplicationReceiver
    {
        /// <summary>
        /// Gets or sets the receiver .NET type. 
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{TKey,TValue}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type of the receiver.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the sender filter. It can be a regex.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the destination filter. It can be a regex.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the settings to be injected to the startup and receivers types.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public IDictionary<string, object> Settings { get; set; }
    }
}