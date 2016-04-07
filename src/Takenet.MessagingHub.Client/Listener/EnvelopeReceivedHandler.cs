using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    internal abstract class EnvelopeReceivedHandler
    {
        private readonly EnvelopeListenerRegistrar _registrar;

        protected Sender.IMessagingHubSender Sender { get; }

        protected EnvelopeReceivedHandler(Sender.IMessagingHubSender sender, EnvelopeListenerRegistrar registrar)
        {
            Sender = sender;
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
                                CallReceiver(r, envelope, CancellationToken.None)))
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

        private Task CallReceiver<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken token)
            where TEnvelope : Envelope
        {
            InjectSenders(envelopeReceiver);
            return ReceiveAsync(envelopeReceiver, envelope, token);
        }

        protected virtual Task ReceiveAsync<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken token)
            where TEnvelope : Envelope
        {
            return envelopeReceiver.ReceiveAsync(envelope, token);
        }

        private void InjectSenders(object receiver)
        {
            if (!(receiver is EnvelopeReceiverBase))
                return;

            var receiverBase = ((EnvelopeReceiverBase)receiver);
            receiverBase.Sender = Sender;
        }
    }

    internal class MessageReceivedHandler : EnvelopeReceivedHandler
    {
        public MessageReceivedHandler(Sender.IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }

        protected override async Task ReceiveAsync<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken token)
        {
            var message = envelope as Message;
            try
            {
                await base.ReceiveAsync(envelopeReceiver, envelope, token);
                await Sender.SendNotificationAsync(message.ToConsumedNotification(), token);
            }
            catch (Exception e)
            {
                var reason = new Reason
                {
                    Code = ReasonCodes.APPLICATION_ERROR,
                    Description = e.Message
                };
                await Sender.SendNotificationAsync(
                    message.ToFailedNotification(reason), token);
                throw;
            }
        }
    }

    internal class NotificationReceivedHandler : EnvelopeReceivedHandler
    {
        public NotificationReceivedHandler(Sender.IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }
    }

    internal class CommandReceivedHandler : EnvelopeReceivedHandler
    {
        public CommandReceivedHandler(Sender.IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }
    }
}
