using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.BasicSample
{
    public class PrintNotificationReceiver : NotificationReceiverBase
    {
        public override Task ReceiveAsync(Notification notification)
        {
            Console.WriteLine("Notification of {0} event received. Reason: {1}", notification.Event, notification.Reason);
            return Task.FromResult(0);
        }
    }
}
