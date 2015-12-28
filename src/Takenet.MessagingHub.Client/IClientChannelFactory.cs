using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Factory for Messaging Hub client channels
    /// </summary>
    internal interface IClientChannelFactory
    {
        Task<IPersistentClientChannel> CreatePersistentClientChannelAsync(Uri endpoint, TimeSpan sendTimeout, Identity identity, Authentication authentication);
    }
}
