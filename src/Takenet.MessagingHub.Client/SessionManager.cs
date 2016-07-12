using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace Takenet.MessagingHub.Client
{
    public class SessionManager : ISessionManager
    {
        private readonly IBucketExtension _bucketExtension;
        private static readonly TimeSpan SessionExpiration = TimeSpan.FromMinutes(30);

        public SessionManager(IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
        }

        public Task ClearSessionAsync(Node node, CancellationToken cancellationToken) => 
            _bucketExtension.DeleteAsync(GetSessionKey(node), cancellationToken);

        public async Task AddVariableAsync(Node node, string key, string value, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var session = await GetOrCreateSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session.Variables == null) session.Variables = new Dictionary<string, string>();
            session.Variables[key] = value;
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.Variables == null || !session.Variables.ContainsKey(key)) return null;
            return session.Variables[key];
        }

        public async Task RemoveVariableAsync(Node node, string key, CancellationToken cancellationToken)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.Variables == null || !session.Variables.ContainsKey(key)) return;
            session.Variables.Remove(key);
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddStateAsync(Node node, string state, CancellationToken cancellationToken)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var session = await GetOrCreateSessionAsync(node, cancellationToken).ConfigureAwait(false);
            var states = session.States?.ToList() ?? new List<string>();
            states.Add(state);
            session.States = states.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> HasStateAsync(Node node, string state, CancellationToken cancellationToken)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.States == null) return false;
            return session.States.Contains(state, StringComparer.OrdinalIgnoreCase);
        }

        public async Task RemoveStateAsync(Node node, string state, CancellationToken cancellationToken)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var session = await GetSessionAsync(node, cancellationToken).ConfigureAwait(false);
            if (session?.States == null || session.States.Length == 0) return;
            session.States = session.States.Where(s => s.Equals(state, StringComparison.OrdinalIgnoreCase)).ToArray();
            await SaveSessionAsync(node, session, cancellationToken).ConfigureAwait(false);
        }

        public Task<NavigationSession> GetSessionAsync(Node node, CancellationToken cancellationToken) => 
            _bucketExtension.GetAsync<NavigationSession>(GetSessionKey(node), cancellationToken);

        private async Task<NavigationSession> GetOrCreateSessionAsync(Node node, CancellationToken cancellationToken)
        {
            return await GetSessionAsync(node, cancellationToken) ??
                          new NavigationSession()
                          {
                              Identity = node.ToIdentity(),
                              Creation = DateTimeOffset.UtcNow
                          };
        }

        private Task SaveSessionAsync(Node node, NavigationSession session, CancellationToken cancellationToken) => 
            _bucketExtension.SetAsync(GetSessionKey(node), session, SessionExpiration, cancellationToken);

        private static string GetSessionKey(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return $"sessions:{node.ToIdentity()}".ToLowerInvariant();
        }
    }
}