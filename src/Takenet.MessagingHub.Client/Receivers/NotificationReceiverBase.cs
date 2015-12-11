using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public abstract class NotificationReceiverBase : ReceiverBase, INotificationReceiver
    {
        public abstract Task ReceiveAsync(Notification notification);
    }
}
