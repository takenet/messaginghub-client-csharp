using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaMessageReceiver : IMessageReceiver
    {
        public Func<MessagingHubSender, Message, CancellationToken, Task> OnMessageReceived { get; set; }

        public LambdaMessageReceiver(Func<MessagingHubSender, Message, CancellationToken, Task> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(MessagingHubSender sender, Message envelope, CancellationToken token)
        {
            return OnMessageReceived?.Invoke(sender, envelope, token);
        }
    }
}