using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        private Func<Notification, IMessagingHubSender, CancellationToken, Task> OnNotificationReceived { get; }

        public LambdaNotificationReceiver(Func<Notification, IMessagingHubSender, CancellationToken, Task> onNotificationReceived)
        {
            if (onNotificationReceived == null) throw new ArgumentNullException(nameof(onNotificationReceived));
            OnNotificationReceived = onNotificationReceived;
        }

        public Task ReceiveAsync(Notification notification, IMessagingHubSender sender, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnNotificationReceived?.Invoke(notification, sender, cancellationToken);
        }
    }
}