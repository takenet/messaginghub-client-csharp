using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Provides the management of states for filtering message and notification receivers registered in the application.
    /// </summary>
    public sealed class MemoryStateManager : IStateManager
    {
        private static MemoryStateManager _instance;
        private readonly ObjectCache _nodeStateCache;

        private MemoryStateManager()
        {
            _nodeStateCache = new MemoryCache(nameof(MemoryStateManager));
            StateTimeout = TimeSpan.FromMinutes(30);
        }

        /// <summary>
        /// Gets or sets the state expiration timeout.
        /// </summary>
        /// <value>
        /// The state timeout.
        /// </value>
        public TimeSpan StateTimeout { get; set; }

        /// <summary>
        /// Gets the last known identity state.
        /// </summary>
        /// <param name="identity">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Task<string> GetStateAsync(Identity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            return Task.FromResult(_nodeStateCache.Get(GetCacheKey(identity)) as string ?? Constants.DEFAULT_STATE);
        }

        /// <summary>
        /// Sets the identity state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="state">The state.</param>
        public Task SetStateAsync(Identity identity, string state)
        {
            SetState(identity, state, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets the identity state to the default value.
        /// </summary>
        /// <param name="identity">The node.</param>
        public Task ResetStateAsync(Identity identity)
        {
            return SetStateAsync(identity, Constants.DEFAULT_STATE);
        }

        /// <summary>
        /// Occurs when a identity state is changed.
        /// This event should be used to synchronize multiple application instances states.
        /// </summary>
        public event EventHandler<StateEventArgs> StateChanged;

        internal void SetState(Identity identity, string state, bool raiseEvent)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Equals(Constants.DEFAULT_STATE, StringComparison.OrdinalIgnoreCase))
            {
                _nodeStateCache.Remove(GetCacheKey(identity));
            }
            else
            {
                _nodeStateCache.Set(GetCacheKey(identity), state, new CacheItemPolicy()
                {
                    SlidingExpiration = StateTimeout
                });
            }

            if (raiseEvent)
            {
                StateChanged?.Invoke(this, new StateEventArgs(identity, state));
            }
        }

        private static string GetCacheKey(Identity identity)
        {
            return $"{identity.Name}@{identity.Domain}".ToLowerInvariant();
        }
    }
}
