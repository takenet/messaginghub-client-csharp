using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Sender
{
    /// <summary>
    /// Send envelopes through a Messaging Hub connection
    /// </summary>
    public interface IMessagingHubSender
    {
        /// <summary>
        /// Send a command through the Messaging Hub
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation. When completed, it will contain the command response</returns>
        Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        Task SendMessageAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a notification through the Messaging Hub
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <param name="cancellationToken">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken));
    }
}