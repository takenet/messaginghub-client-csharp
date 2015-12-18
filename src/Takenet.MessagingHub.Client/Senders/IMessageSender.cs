using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Send messages to the Messaging Hub
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>A task representing the sending operation</returns>

        Task SendMessageAsync(Message message);
    }
}
