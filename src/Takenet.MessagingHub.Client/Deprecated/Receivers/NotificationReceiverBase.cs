using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Base envelope receiver
    /// </summary>
    [Obsolete]
    public abstract class NotificationReceiverBase : EnvelopeReceiverBase, INotificationReceiver
    {
        public abstract Task ReceiveAsync(Notification notification);
    }
}
