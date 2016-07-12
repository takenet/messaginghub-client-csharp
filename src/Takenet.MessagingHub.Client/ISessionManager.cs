using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    public interface ISessionManager
    {
        Task<NavigationSession> GetSessionAsync(Node node, CancellationToken cancellationToken);

        Task ClearSessionAsync(Node node, CancellationToken cancellationToken);

        Task AddVariableAsync(Node node, string key, string value, CancellationToken cancellationToken);

        Task<string> GetVariableAsync(Node node, string key, CancellationToken cancellationToken);

        Task RemoveVariableAsync(Node node, string key, CancellationToken cancellationToken);

        Task AddStateAsync(Node node, string state, CancellationToken cancellationToken);

        Task<bool> HasStateAsync(Node node, string state, CancellationToken cancellationToken);

        Task RemoveStateAsync(Node node, string state, CancellationToken cancellationToken);
    }
}
