using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Proxy used to send messages to the Messaging Hub
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Send a message to the Message Hube
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>A task representing the async operation</returns>
        Task SendMessageAsync(Message message);
    }
}
