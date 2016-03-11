using System;
using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Host
{
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
        /// Gets or sets the settings to be injected to the startup and receivers types.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public IDictionary<string, object> Settings { get; set; }
    }
}