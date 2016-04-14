using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Connection
{
    /// <summary>
    /// Represents a connection with the messaging hub. Use the <see cref="MessagingHubConnectionBuilder"/> to build a connection
    /// </summary>
    public interface IMessagingHubConnection
    {
        /// <summary>
        /// Indicates whether or not a the connection is active
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Maximum number of retries when failing to connect to the Messaging Hub
        /// </summary>
        int MaxConnectionRetries { get; set; }

        /// <summary>
        /// Time to wait before a timeout exception is raised when trying to send envelopes through this connection
        /// </summary>
        TimeSpan SendTimeout { get; }

        /// <summary>
        /// Channel through which the envelopes are sent and received
        /// </summary>
        IOnDemandClientChannel OnDemandClientChannel { get; }

        /// <summary>
        /// Activate the connection with the Messaging Hub
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Disconnects from the Messaging Hub
        /// </summary>
        Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}