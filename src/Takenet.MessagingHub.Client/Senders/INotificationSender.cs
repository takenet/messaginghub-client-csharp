using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Proxy used to send notifications to the Messaging Hub
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Send a notification to the Message Hube
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <returns>A task representing the async operation</returns>
        Task SendNotificationAsync(Notification notification);
    }
}