using System;
using System.Collections.Generic;
using System.IO;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Takenet.MessagingHub.Client.Listener;

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
        public MessageApplicationReceiver[] MessageReceivers { get; set; }

        /// <summary>
        /// Gets or sets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public NotificationApplicationReceiver[] NotificationReceivers { get; set; }

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

        /// <summary>
        /// Gets or sets the session encryption mode to be used
        /// </summary>
        /// <value>
        /// The encryption mode.
        /// </value>
        public SessionEncryption? SessionEncryption { get; set; }

        /// <summary>
        /// Gets or sets the session compression mode to be used
        /// </summary>
        /// <value>
        /// The compression mode.
        /// </value>
        public SessionCompression? SessionCompression { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Application"/> from a JSON input.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Application ParseFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            settings.Converters.Add(new StringEnumConverter
            {
                CamelCaseText = true,
                AllowIntegerValues = true
            });

            return JsonConvert.DeserializeObject<Application>(json, settings);
        }

        /// <summary>
        /// Creates an instance of <see cref="Application" /> from a JSON file.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns></returns>
        public static Application ParseFromJsonFile(string filePath) => ParseFromJson(File.ReadAllText(filePath));
        
    }
}
