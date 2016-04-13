using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    public class LambdaMessageReceiver : IMessageReceiver
    {
        private Func<IMessagingHubSender, Message, CancellationToken, Task> OnMessageReceived { get; }

        public LambdaMessageReceiver(Func<IMessagingHubSender, Message, CancellationToken, Task> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException(nameof(onMessageReceived));
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(IMessagingHubSender sender, Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnMessageReceived?.Invoke(sender, envelope, cancellationToken);
        }
    }
}