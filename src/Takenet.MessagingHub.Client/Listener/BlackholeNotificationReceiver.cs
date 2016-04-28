using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Notification receiver that simply ignores the received notification
    /// </summary>
    public class BlackholeNotificationReceiver : INotificationReceiver
    {
        public Task ReceiveAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}