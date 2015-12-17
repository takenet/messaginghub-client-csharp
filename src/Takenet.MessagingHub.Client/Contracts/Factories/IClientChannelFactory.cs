using Lime.Protocol.Client;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Factory for Messaging Hub client channels
    /// </summary>
    internal interface IClientChannelFactory
    {
        /// <summary>
        /// Creates a new client channel and connects to a Messaging Hub endpoint
        /// </summary>
        /// <param name="endpoint">Uri endpoint for the Messaging Hub</param>
        /// <returns>A new client channel connected to the given endpoint</returns>
        Task<IClientChannel> CreateClientChannelAsync(Uri endpoint);
    }
}
