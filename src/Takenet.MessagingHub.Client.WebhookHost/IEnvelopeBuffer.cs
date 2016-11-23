using System.Threading.Tasks.Dataflow;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.WebhookHost
{
    public interface IEnvelopeBuffer
    {
        BufferBlock<Message> Messages { get; }
        BufferBlock<Notification> Notifications { get; }
        BufferBlock<Command> Commands { get; }
    }
}