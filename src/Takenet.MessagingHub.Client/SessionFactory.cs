using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using System.Threading;
using Lime.Transport.Tcp;
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
