using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Base message receiver
    /// </summary>
    public abstract class MessageReceiverBase : EnvelopeReceiverBase, IMessageReceiver
    {
        public abstract Task ReceiveAsync(Message message);
    }
}
