using System.Threading.Tasks;
using Lime.Protocol;
using System;
using Takenet.MessagingHub.Client.Lime;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Senders
{
    internal abstract class EnvelopeSenderBase<TEnvelope> : IEnvelopeSender<TEnvelope>
        where TEnvelope : Envelope
    {
        private readonly IEnvelopeProcessor<TEnvelope> _processor;
        private readonly TimeSpan _timeout;

        protected EnvelopeSenderBase(IEnvelopeProcessor<TEnvelope> processor, TimeSpan timeout)
        {
            _processor = processor;
            _timeout = timeout;
        }

        public Task<TEnvelope> SendAsync(TEnvelope envelope) => _processor.SendAsync(envelope, _timeout);
    }
}
