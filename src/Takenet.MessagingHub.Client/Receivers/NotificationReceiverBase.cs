using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Base envelope receiver
    /// </summary>
    public abstract class NotificationReceiverBase : EnvelopeReceiverBase, INotificationReceiver
    {
        public abstract Task ReceiveAsync(Notification notification);
    }
}
