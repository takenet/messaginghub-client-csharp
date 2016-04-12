using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        public Func<MessagingHubSender, Notification, CancellationToken, Task> OnMessageReceived { get; set; }

        public LambdaNotificationReceiver(Func<MessagingHubSender, Notification, CancellationToken, Task> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(MessagingHubSender sender, Notification envelope, CancellationToken token)
        {
            return OnMessageReceived?.Invoke(sender, envelope, token);
        }
    }
}