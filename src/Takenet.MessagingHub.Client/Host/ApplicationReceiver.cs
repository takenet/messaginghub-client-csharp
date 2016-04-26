using System;
using System.Collections.Generic;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Host
{
    public class ApplicationReceiver : SettingsContainer
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
        /// Gets or sets the message to be sent in case of the occurrences of the specified envelope filter.
        /// This overrides the receiver type definition, if present.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public DocumentDefinition Response { get; set; }
    }
}