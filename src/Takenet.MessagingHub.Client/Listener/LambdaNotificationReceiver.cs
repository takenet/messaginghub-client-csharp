using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        public Func<IMessagingHubSender, Notification, CancellationToken, Task> OnMessageReceived { get; set; }

        public LambdaNotificationReceiver(Func<IMessagingHubSender, Notification, CancellationToken, Task> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(IMessagingHubSender sender, Notification envelope, CancellationToken cancellationToken)
        {
            return OnMessageReceived?.Invoke(sender, envelope, cancellationToken);
        }
    }
}