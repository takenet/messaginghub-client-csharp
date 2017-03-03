using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Provides the management of states for filtering message and notification receivers registered in the application.
    /// </summary>
    public sealed class BucketStateManager : IStateManager
    {
        public const string StateSessionKey = "$stateManagerState$";

        private static readonly TimeSpan CommandTimeout = TimeSpan.FromMilliseconds(5000);

        private readonly IBucketExtension _bucket;

        public BucketStateManager(IBucketExtension bucket)
        {
            StateTimeout = TimeSpan.FromMinutes(30);
            _bucket = bucket;
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
        public async Task<string> GetStateAsync(Identity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            using (var cts = new CancellationTokenSource(CommandTimeout))
            {
                return (await _bucket.GetAsync<StateDocument>(GetKey(identity), cts.Token))?.State ?? Constants.DEFAULT_STATE;
            }
        }

        /// <summary>
        /// Sets the identity state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="state">The state.</param>
        public Task SetStateAsync(Identity identity, string state)
        {
            return SetStateAsync(identity, state, true);
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

        internal async Task SetStateAsync(Identity identity, string state, bool raiseEvent)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Equals(Constants.DEFAULT_STATE, StringComparison.OrdinalIgnoreCase))
            {
                using (var cts = new CancellationTokenSource(CommandTimeout))
                {
                    await _bucket.DeleteAsync(GetKey(identity), cts.Token);
                }
            }
            else
            {
                using (var cts = new CancellationTokenSource(CommandTimeout))
                {
                    await _bucket.SetAsync(GetKey(identity), new StateDocument { State = state }, StateTimeout, cts.Token);
                }
            }

            if (raiseEvent)
            {
                StateChanged?.Invoke(this, new StateEventArgs(identity, state));
            }
        }

        private static string GetKey(Identity identity)
        {
            return $"{StateSessionKey}_{identity.Name.ToLowerInvariant()}@{identity.Domain.ToLowerInvariant()}";
        }
    }
}
