using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Send commands to the Messaging Hub
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Send a command through the Messaging Hub
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <returns>A task representing the sending operation. When completed, it will contain the command response</returns>
        Task<Command> SendCommandAsync(Command command);
    }
}