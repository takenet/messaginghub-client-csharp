using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        public Action<Notification, CancellationToken> OnMessageReceived { get; set; }

        public LambdaNotificationReceiver(Action<Notification, CancellationToken> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(MessagingHubSender sender, Notification envelope, CancellationToken token)
        {
            return Task.Run(() => OnMessageReceived?.Invoke(envelope, token), token);
        }
    }
}