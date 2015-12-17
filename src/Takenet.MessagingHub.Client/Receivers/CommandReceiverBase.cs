using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public abstract class CommandReceiverBase : EnvelopeReceiverBase, ICommandReceiver
    {
        public abstract Task ReceiveAsync(Command command);
    }
}
