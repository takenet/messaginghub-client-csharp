using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface INotificationReceiver
    {
        Task ReceiveAsync(Notification notification);
    }
}