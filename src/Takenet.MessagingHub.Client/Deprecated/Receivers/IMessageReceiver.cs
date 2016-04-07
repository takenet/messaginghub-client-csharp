using System;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated.Receivers
{
    /// <summary>
    /// Specialization of <see cref="IEnvelopeReceiver{TEnvelope}"/> for messages
    /// </summary>
    [Obsolete]
    public interface IMessageReceiver : IEnvelopeReceiver<Message>
    {
    }
}
