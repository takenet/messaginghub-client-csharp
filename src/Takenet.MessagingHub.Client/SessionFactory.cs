using System;
using System.Threading.Tasks;
using Lime.Protocol.Client;
using System.Threading;
using Lime.Protocol;
using Lime.Protocol.Security;

namespace Takenet.MessagingHub.Client
{
    internal class SessionFactory : ISessionFactory
    {
        public async Task<Session> CreateSessionAsync(IClientChannel clientChannel, Identity identity, Authentication authentication)
        {
            return await clientChannel.EstablishSessionAsync(
                            _ => SessionCompression.None,
                            _ => SessionEncryption.TLS,
                            identity,
                            (_, __) => authentication,
                            Environment.MachineName,
                            CancellationToken.None);
        }
    }
}
