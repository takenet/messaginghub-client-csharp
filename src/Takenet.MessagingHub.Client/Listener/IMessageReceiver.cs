using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Specialization of <see cref="IEnvelopeReceiver{TEnvelope}"/> for messages
    /// </summary>
    public interface IMessageReceiver : IEnvelopeReceiver<Message>
    {
    }
}
