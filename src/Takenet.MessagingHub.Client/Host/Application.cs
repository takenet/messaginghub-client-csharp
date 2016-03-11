﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Defines the configuration type for the application.json file.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>
        /// The domain.
        /// </value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>
        /// The access key.
        /// </value>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the send timeout, in milliseconds.
        /// </summary>
        /// <value>
        /// The send timeout.
        /// </value>
        public int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public ApplicationMessageReceiver[] MessageReceivers { get; set; }

        /// <summary>
        /// Gets or sets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public ApplicationNotificationReceiver[] NotificationReceivers { get; set; }

        /// <summary>
        /// Gets or sets the type for the startup .NET type. It must implement <see cref="IStartable"/> or <see cref="IFactory{IStartable}"/>.
        /// The start is called before the start of the sender.
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{string, object}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type of the startup.
        /// </value>
        public string StartupType { get; set; }

        /// <summary>
        /// Gets or sets the settings to be injected to the startup and receivers types.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public IDictionary<string, object> Settings { get; set; }
    }
}
