using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaMessageReceiver : IMessageReceiver
    {
        private Func<Message, IMessagingHubSender, CancellationToken, Task> OnMessageReceived { get; }

        public LambdaMessageReceiver(Func<Message, IMessagingHubSender, CancellationToken, Task> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException(nameof(onMessageReceived));
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(Message message, IMessagingHubSender sender, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnMessageReceived?.Invoke(message, sender, cancellationToken);
        }
    }
}