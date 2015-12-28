using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    internal interface ILimeSessionProvider
    {
        Task<Session> EstablishSessionAsync(IPersistentClientChannel clientChannel, CancellationToken cancellationToken);

        Task FinishSessionAsync(IPersistentClientChannel clientChannel, CancellationToken cancellationToken);

        bool IsSessionEstablished(IPersistentClientChannel clientChannel);
    }
}
