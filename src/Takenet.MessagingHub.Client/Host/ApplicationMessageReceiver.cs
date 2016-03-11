using System;
using System.Collections.Generic;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Host
{
    public class ApplicationMessageReceiver
    {
        /// <summary>
        /// Gets or sets the receiver .NET type. It must implement <see cref="IMessageReceiver"/> or <see cref="IFactory{IMessageReceiver}"/>.
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{TKey,TValue}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public string MediaType { get; set; }

    }
}