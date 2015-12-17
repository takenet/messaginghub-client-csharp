using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    public abstract class MessageReceiverBase : EnvelopeReceiverBase, IMessageReceiver
    {
        public abstract Task ReceiveAsync(Message message);
    }
}
