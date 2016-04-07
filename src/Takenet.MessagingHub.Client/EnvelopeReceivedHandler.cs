using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    internal abstract class EnvelopeReceivedHandler
    {
        private readonly EnvelopeListenerRegistrar _registrar;
        private readonly IEnvelopeSender _envelopeSender;

        protected EnvelopeReceivedHandler(IEnvelopeSender envelopeSender, EnvelopeListenerRegistrar registrar)
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

        private Task CallReceiver<TEnvelope>(IEnvelopeSender envelopeSender,
            IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope)
            where TEnvelope : Envelope
        {
            InjectSenders(envelopeSender, envelopeReceiver);
            return ReceiveAsync(envelopeReceiver, envelopeSender, envelope);
        }

        protected virtual Task ReceiveAsync<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, IEnvelopeSender envelopeSender, TEnvelope envelope)
            where TEnvelope : Envelope
        {
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

    internal class MessageReceivedHandler : EnvelopeReceivedHandler
    {
        public MessageReceivedHandler(IEnvelopeSender envelopeSender, EnvelopeListenerRegistrar registrar) : base(envelopeSender, registrar)
        {
        }

        protected override Task ReceiveAsync<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, IEnvelopeSender envelopeSender, TEnvelope envelope)
        {
            var message = envelope as Message;
            try
            {
                base.ReceiveAsync(envelopeReceiver, envelopeSender, envelope);
                return envelopeSender.SendNotificationAsync(message.ToConsumedNotification());
            }
            catch (Exception e)
            {
                envelopeSender.SendNotificationAsync(
                    message.ToFailedNotification(new Reason
                    {
                        Code = ReasonCodes.APPLICATION_ERROR,
                        Description = e.Message
                    }));
                throw;
            }
        }
    }

    internal class NotificationReceivedHandler : EnvelopeReceivedHandler
    {
        public NotificationReceivedHandler(IEnvelopeSender envelopeSender, EnvelopeListenerRegistrar registrar) : base(envelopeSender, registrar)
        {
        }
    }

    internal class CommandReceivedHandler : EnvelopeReceivedHandler
    {
        public CommandReceivedHandler(IEnvelopeSender envelopeSender, EnvelopeListenerRegistrar registrar) : base(envelopeSender, registrar)
        {
        }
    }
}
