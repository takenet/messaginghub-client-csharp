using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal class LimeSessionProvider : ILimeSessionProvider
    {
        private Identity _identity;
        private Authentication _authentication;
        private Uri _endPoint;

        public LimeSessionProvider(Uri endPoint, Identity identity, Authentication authentication)
        {
            _endPoint = endPoint;
            _identity = identity;
            _authentication = authentication;
        }
        
        public async Task<Session> EstablishSessionAsync(IPersistentClientChannel clientChannel, CancellationToken cancellationToken)
        {
            if (IsSessionEstablished(clientChannel))
            {
                return clientChannel.Session;
            }

            await clientChannel.Transport.OpenAsync(_endPoint, cancellationToken).ConfigureAwait(false);

            if (!clientChannel.Transport.IsConnected)
            {
                throw new Exception("Could not open connection");
            }

            return await clientChannel.EstablishSessionAsync(
                            _ => SessionCompression.None,
                            _ => SessionEncryption.TLS,
                            _identity,
                            (_, __) => _authentication,
                            Environment.MachineName,
                            cancellationToken);
        }

        public async Task FinishSessionAsync(IPersistentClientChannel clientChannel, CancellationToken cancellationToken)
        {
            if (IsSessionEstablished(clientChannel))
            {
                await clientChannel.SendFinishingSessionAsync();
            }

            if (clientChannel.Transport.IsConnected)
            {
                await clientChannel.Transport.CloseAsync(cancellationToken);
            }
        }

        public bool IsSessionEstablished(IPersistentClientChannel clientChannel)
        {
            return clientChannel.Transport.IsConnected && clientChannel.State == SessionState.Established;
        }
    }
}
