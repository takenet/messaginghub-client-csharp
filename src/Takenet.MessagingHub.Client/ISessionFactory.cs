using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal interface ISessionFactory
    {
        Task<Session> CreateSessionAsync(IClientChannel clientChannel, string login, Authentication authentication);
    }
}
