using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    internal class EnvelopeReceivedHandler
    {
        private readonly EnvelopeListenerRegistrar _registrar;
        private readonly IEnvelopeSender _envelopeSender;

        public EnvelopeReceivedHandler(IEnvelopeSender envelopeSender, EnvelopeListenerRegistrar registrar)
        {
            _envelopeSender = envelopeSender;
            _registrar = registrar;
        }

        public async Task<bool> HandleAsync<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope
        {
            try
            {
                await Task
                        .WhenAll(
                            _registrar.GetReceiversFor(envelope).Select(r =>
                                CallReceiver(_envelopeSender, r, envelope)))
                        .ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                Trace.TraceError(ex.ToString());
                return true;
            }
        }

        private static Task CallReceiver<TEnvelope>(IEnvelopeSender envelopeSender,
            IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope)
            where TEnvelope : Envelope
        {
            InjectSenders(envelopeSender, envelopeReceiver);
            return envelopeReceiver.ReceiveAsync(envelope);
        }

        private static void InjectSenders(IEnvelopeSender envelopeSender, object receiver)
        {
            if (!(receiver is EnvelopeReceiverBase))
                return;

            var receiverBase = ((EnvelopeReceiverBase)receiver);
            receiverBase.EnvelopeSender = envelopeSender;
        }
    }

}
