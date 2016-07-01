using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;
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

        public async Task<bool> HandleAsync<TEnvelope>(TEnvelope envelope, CancellationToken cancellationToken)
            where TEnvelope : Envelope, new()
        {
            try
            {
                await CallReceiversAsync(envelope, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                Trace.TraceError(ex.ToString());
                return true;
            }
        }

        protected virtual async Task CallReceiversAsync<TEnvelope>(TEnvelope envelope, CancellationToken cancellationToken) where TEnvelope : Envelope, new()
        {
            var receiverGroups = _registrar
                .GetReceiversFor(envelope)
                .GroupBy(r => r.Priority)
                .OrderBy(r => r.Key);

            foreach (var receivers in receiverGroups)
            {
                await
                    Task.WhenAll(
                        receivers.Select(r => CallReceiverAsync(r.ReceiverFactory(), envelope, cancellationToken)));
            }            
        }

        protected Task CallReceiverAsync<TEnvelope>(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken cancellationToken)
            where TEnvelope : Envelope
        {
            return envelopeReceiver.ReceiveAsync(envelope, cancellationToken);
        }
    }

    internal class MessageReceivedHandler : EnvelopeReceivedHandler
    {
        public MessageReceivedHandler(IMessagingHubSender sender, EnvelopeListenerRegistrar registrar) : base(sender, registrar)
        {
        }

        protected override async Task CallReceiversAsync<TEnvelope>(TEnvelope envelope, CancellationToken cancellationToken)
        {
            var message = envelope as Message;
            try
            {
                await base.CallReceiversAsync(envelope, cancellationToken);
                await Sender.SendNotificationAsync(message.ToConsumedNotification(), CancellationToken.None);
            }
            catch (LimeException e)
            {
                await Sender.SendNotificationAsync(message.ToFailedNotification(e.Reason), CancellationToken.None);
                throw;
            }
            catch (Exception e)
            {
                var reason = new Reason
                {
                    Code = ReasonCodes.APPLICATION_ERROR,
                    Description = e.Message
                };
                await Sender.SendNotificationAsync(
                    message.ToFailedNotification(reason), CancellationToken.None);
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
