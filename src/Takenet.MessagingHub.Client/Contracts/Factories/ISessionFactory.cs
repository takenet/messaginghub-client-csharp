using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Factory for Messaging Hub sessions
    /// </summary>
    internal interface ISessionFactory
    {
        /// <summary>
        /// Creates and establishes a new session
        /// </summary>
        /// <param name="clientChannel">Client channel</param>
        /// <param name="identity">Identity of the client</param>
        /// <param name="authentication">Authentication of the client</param>
        /// <returns></returns>
        Task<Session> CreateSessionAsync(IClientChannel clientChannel, Identity identity, Authentication authentication);
    }
}
