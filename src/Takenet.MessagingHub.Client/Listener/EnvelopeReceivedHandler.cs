using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Messages;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal abstract class EnvelopeReceivedHandler
    {
        private readonly EnvelopeListenerRegistrar _registrar;

        protected IMessagingHubSender Sender { get; }

        protected EnvelopeReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar)
        {
            Sender = sender;
            _registrar = registrar;
        }

        public async Task<bool> HandleAsync<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            try
            {
                await Task
                        .WhenAll(
                            _registrar.GetReceiversFor(envelope).Select(r =>
                                CallReceiver(r.ReceiverFactory(), envelope, r.CancellationToken)))
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

        protected virtual Task CallReceiver<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken cancellationToken)
            where TEnvelope : Envelope
        {
            return envelopeReceiver.ReceiveAsync(envelope, Sender, cancellationToken);
        }
    }

    internal class MessageReceivedHandler : EnvelopeReceivedHandler
    {
        public MessageReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }

        protected override async Task CallReceiver<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken cancellationToken)
        {
            var message = envelope as Message;
            try
            {
                await base.CallReceiver(envelopeReceiver, envelope, cancellationToken);
                await Sender.SendNotificationAsync(message.ToConsumedNotification(), cancellationToken);
            }
            catch (Exception e)
            {
                var reason = new Reason
                {
                    Code = ReasonCodes.APPLICATION_ERROR,
                    Description = e.Message
                };
                await Sender.SendNotificationAsync(
                    message.ToFailedNotification(reason), cancellationToken);
                throw;
            }
        }
    }

    internal class NotificationReceivedHandler : EnvelopeReceivedHandler
    {
        public NotificationReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }
    }
}
