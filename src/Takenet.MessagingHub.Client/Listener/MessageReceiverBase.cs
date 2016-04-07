using System.Threading;
using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Base message receiver
    /// </summary>
    public abstract class MessageReceiverBase : EnvelopeReceiverBase, IMessageReceiver
    {
        public abstract Task ReceiveAsync(Message message, CancellationToken token);
    }
}
