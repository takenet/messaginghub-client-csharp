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
        Task EstablishSessionAsync(IClientChannel clientChannel, Uri endPoint, Identity identity, Authentication authentication, CancellationToken cancellationToken);

        Task FinishSessionAsync(IClientChannel clientChannel, CancellationToken cancellationToken);

        bool IsSessionEstablished(IClientChannel clientChannel);
    }
}
