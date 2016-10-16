using Lime.Protocol;
using System.Threading.Tasks.Dataflow;

namespace $rootnamespace$
{
    public interface IEnvelopeBuffer
    {
        BufferBlock<Message> Messages { get; }
        BufferBlock<Notification> Notifications { get; }
        BufferBlock<Command> Commands { get; }
    }
}