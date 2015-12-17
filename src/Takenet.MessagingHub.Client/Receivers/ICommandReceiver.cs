using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public interface ICommandReceiver : IEnvelopeReceiver<Command>
    {
    }
}