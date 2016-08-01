using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Provides the management of states for filtering message and notification receivers registered in the application.
    /// </summary>
    public sealed class StateManager
    {
        public const string DEFAULT_STATE = "default";
        
        private static readonly object SyncRoot = new object();
        private static StateManager _instance;
        private readonly ObjectCache _nodeStateCache;

        private StateManager()
        {
            _nodeStateCache = new MemoryCache(nameof(StateManager));
            StateTimeout = TimeSpan.FromMinutes(30);
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static StateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new StateManager();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the state expiration timeout.
        /// </summary>
        /// <value>
        /// The state timeout.
        /// </value>
        public TimeSpan StateTimeout { get; set; }

        /// <summary>
        /// Gets the last known node state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public string GetState(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return _nodeStateCache.Get(GetCacheKey(node)) as string ?? DEFAULT_STATE;
        }

        /// <summary>
        /// Sets the node state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="state">The state.</param>
        public void SetState(Node node, string state)
        {
            SetState(node, state, true);
        }

        /// <summary>
        /// Resets the node state to the default value.
        /// </summary>
        /// <param name="node">The node.</param>
        public void ResetState(Node node)
        {
            SetState(node, DEFAULT_STATE);
        }

        /// <summary>
        /// Occurs when a node state is changed.
        /// This event should be used to synchronize multiple application instances states.
        /// </summary>
        public EventHandler<StateEventArgs> StateChanged;

        internal void SetState(Node node, string state, bool raiseEvent)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Equals(DEFAULT_STATE, StringComparison.OrdinalIgnoreCase))
            {
                _nodeStateCache.Remove(GetCacheKey(node));
            }
            else
            {
                _nodeStateCache.Set(GetCacheKey(node), state, new CacheItemPolicy()
                {
                    SlidingExpiration = StateTimeout
                });                
            }

            if (raiseEvent)
            {                
                StateChanged?.Invoke(this, new StateEventArgs(node, state));
            }
        }

        private static string GetCacheKey(Node node) => node.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Represents an event for the user state.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class StateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEventArgs"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public StateEventArgs(Node node, string state)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (state == null) throw new ArgumentNullException(nameof(state));
            Node = node;
            State = state;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public Node Node { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; }
    }
}
