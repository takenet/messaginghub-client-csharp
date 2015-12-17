using Lime.Protocol;
using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Sends a command and waits for its response
    /// </summary>
    interface ICommandProcessor
    {
        /// <summary>
        /// Starts listening for envelopes
        /// </summary>
        void StartReceiving();
        
        /// <summary>
        /// Start a task to listening
        /// </summary>
        /// <returns>A task representing the stop operation</returns>
        Task StopReceivingAsync();

        /// <summary>
        /// Send a command and wait for its response
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="timeout">Send operation timeout</param>
        /// <returns>A task representing the stop operation</returns>
        Task<Command> SendAsync(Command command, TimeSpan timeout);
    }
}
