using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(Notification notification);
    }
}