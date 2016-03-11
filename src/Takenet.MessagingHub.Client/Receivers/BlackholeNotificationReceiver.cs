using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Notification receiver that simply ignores the received notification
    /// </summary>
    public class BlackholeNotificationReceiver : INotificationReceiver
    {
        public Task ReceiveAsync(Notification notification)
        {
            return Task.FromResult(0);
        }
    }
}