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
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        string GetState(Identity identity);

        /// <summary>
        /// Sets the node state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="state">The state.</param>
        void SetState(Identity identity, string state);

        /// <summary>
        /// Resets the node state to the default value.
        /// </summary>
        /// <param name="identity">The identity.</param>
        void ResetState(Identity identity);

        /// <summary>
        /// Occurs when a state for a node is changed.
        /// </summary>
        event EventHandler<StateEventArgs> StateChanged;
    }
}