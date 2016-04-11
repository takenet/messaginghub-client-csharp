using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaMessageReceiver : IMessageReceiver
    {
        public Action<Message, CancellationToken> OnMessageReceived { get; set; }

        public LambdaMessageReceiver(Action<Message, CancellationToken> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(MessagingHubSender sender, Message envelope, CancellationToken token)
        {
            return Task.Run(() => OnMessageReceived?.Invoke(envelope, token), token);
        }
    }
}