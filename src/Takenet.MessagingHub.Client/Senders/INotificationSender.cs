using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Senders
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(Notification notification);
    }
}