using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Notification receiver that simply ignores the received notification
    /// </summary>
    public class BlackholeNotificationReceiver : INotificationReceiver
    {
        public Task ReceiveAsync(Notification notification, CancellationToken token)
        {
            return Task.FromResult(0);
        }
    }
}