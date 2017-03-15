using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal abstract class EnvelopeReceivedHandler<TEnvelope> where TEnvelope : Envelope, new()
    {
        private readonly EnvelopeReceiverManager _registrar;
        private readonly ActionBlock<TEnvelope> _envelopeActionBlock;
        private readonly CancellationTokenSource _cts;

        protected IMessagingHubSender Sender { get; }

        protected EnvelopeReceivedHandler(
            IMessagingHubSender sender,
            EnvelopeReceiverManager registrar,
            CancellationTokenSource cts)
        {
            Sender = sender;
            _registrar = registrar;
            _cts = cts;
            _envelopeActionBlock = new ActionBlock<TEnvelope>(async envelope =>
            {
                try
                {
                    await CallReceiversAsync(envelope, _cts.Token);
                }
                catch (Exception ex)
                {
                    //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                    Trace.TraceError(ex.ToString());
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });
        }

        public Task<bool> HandleAsync(TEnvelope envelope, CancellationToken cancellationToken) =>
            _envelopeActionBlock.SendAsync(envelope, cancellationToken);

        protected virtual async Task CallReceiversAsync(TEnvelope envelope, CancellationToken cancellationToken)
        {
            var receivers = await _registrar.GetReceiversAsync(envelope);
            // Gets the first non empty group, ordered by priority
            var receiverGroup = receivers
                                .GroupBy(r => r.Priority)
                                .OrderBy(r => r.Key)
                                .First(r => r.Any());

            await Task.WhenAll(
                    receiverGroup.Select(r => CallReceiverAsync(r.ReceiverFactory(), envelope, cancellationToken)));
        }

        protected Task CallReceiverAsync(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken cancellationToken)
        {
            return envelopeReceiver.ReceiveAsync(envelope, cancellationToken);
        }
    }
}
