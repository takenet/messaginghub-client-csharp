using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Send notifications through the Messaging Hub
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Send a notification through the Messaging Hub
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <returns>A task representing the sending operation</returns>
        Task SendNotificationAsync(Notification notification);
    }
}