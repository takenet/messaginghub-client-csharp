using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        public Action<Notification, CancellationToken> OnMessageReceived { get; set; }

        public LambdaNotificationReceiver(Action<Notification, CancellationToken> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(Notification envelope, CancellationToken token)
        {
            return Task.Run(() => OnMessageReceived?.Invoke(envelope, token), token);
        }
    }
}