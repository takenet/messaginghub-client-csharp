using System;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Defines a service that manages the states for filtering message and notification receivers registered in the application.
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Gets or sets the state expiration timeout.
        /// </summary>
        /// <value>
        /// The state timeout.
        /// </value>
        TimeSpan StateTimeout { get; set; }

        /// <summary>
        /// Gets the last known node state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        string GetState(Node node);

        /// <summary>
        /// Sets the node state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="state">The state.</param>
        void SetState(Node node, string state);

        /// <summary>
        /// Resets the node state to the default value.
        /// </summary>
        /// <param name="node">The node.</param>
        void ResetState(Node node);

        /// <summary>
        /// Occurs when a state for a node is changed.
        /// </summary>
        event EventHandler<StateEventArgs> StateChanged;
    }
}