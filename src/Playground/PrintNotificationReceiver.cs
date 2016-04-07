using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;

namespace Playground
{
    /// <summary>
    /// Example of a notification receiver
    /// </summary>
    public class PrintNotificationReceiver : NotificationReceiverBase
    {
        public override Task ReceiveAsync(Notification notification, CancellationToken token)
        {
            Console.WriteLine("Notification of {0} event received. Reason: {1}", notification.Event, notification.Reason);
            return Task.FromResult(0);
        }
    }
}
