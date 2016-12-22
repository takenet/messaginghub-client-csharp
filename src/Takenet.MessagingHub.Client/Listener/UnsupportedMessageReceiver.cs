using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Message receiver that automatically respond to any message as an unsupported message
    /// </summary>
    public class UnsupportedMessageReceiver : UnsupportedEnvelopeReceiver<Message>
    {
        public UnsupportedMessageReceiver() : base(
            new Reason
            {
                Code = ReasonCodes.MESSAGE_UNSUPPORTED_CONTENT_TYPE,
                Description = "There's no processor available to handle the received message"
            })
        { }
    }

    public class UnsupportedCommandReceiver : UnsupportedEnvelopeReceiver<Command>
    {
        public UnsupportedCommandReceiver() : base(
            new Reason
            {
                Code = ReasonCodes.COMMAND_RESOURCE_NOT_SUPPORTED,
                Description = "There's no resource processor available to handle the received command"
            })
        { }
    }

    public abstract class UnsupportedEnvelopeReceiver<TEnvelope> : IEnvelopeReceiver<TEnvelope>
        where TEnvelope : Envelope
    {
        private Reason _reason;

        protected UnsupportedEnvelopeReceiver(Reason reason)
        {
            _reason = reason;
        }

        public virtual Task ReceiveAsync(TEnvelope enevelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(enevelope.Id) || enevelope.Id == Guid.Empty.ToString())
            {
                throw new LimeException(
                    _reason);
            }

            return Task.CompletedTask;
        }
    }
}