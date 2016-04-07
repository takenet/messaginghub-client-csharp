using System.Threading;
using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Base message receiver
    /// </summary>
    public abstract class NotificationReceiverBase : EnvelopeReceiverBase, INotificationReceiver
    {
        public abstract Task ReceiveAsync(Notification message, CancellationToken token);
    }
}
