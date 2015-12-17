using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Proxy used to send commands to the Messaging Hub
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Send a command to the Message Hube
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <returns>A task representing the async operation</returns>
        Task<Command> SendCommandAsync(Command command);
    }
}