using System.Threading;
using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Base message receiver
    /// </summary>
    public abstract class NotificationReceiverBase : INotificationReceiver
    {
        public abstract Task ReceiveAsync(IMessagingHubSender channel, Notification message, CancellationToken token);
    }
}
