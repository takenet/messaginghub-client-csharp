using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ConcurrentDictionary<Node, string> _nodeStateDictionary;

        private StateManager()
        {
            _nodeStateDictionary = new ConcurrentDictionary<Node, string>();
        }

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
        /// Gets the last known node state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public string GetState(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            string state;
            if (!_nodeStateDictionary.TryGetValue(node, out state))
            {
                state = DEFAULT_STATE;
            }
            return state;
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
                _nodeStateDictionary.TryRemove(node, out state);
            }
            else
            {
                _nodeStateDictionary.AddOrUpdate(
                    node,
                    n => state,
                    (n, o) => state);
            }

            if (raiseEvent)
            {                
                StateChanged?.Invoke(this, new StateEventArgs(node, state));
            }
        }
    }

    public class StateEventArgs : EventArgs
    {
        public StateEventArgs(Node node, string state)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (state == null) throw new ArgumentNullException(nameof(state));
            Node = node;
            State = state;
        }

        public Node Node { get; }

        public string State { get; }
    }
}
