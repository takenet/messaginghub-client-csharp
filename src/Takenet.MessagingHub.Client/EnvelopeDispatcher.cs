using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Dispatch envelopes from a producer to a set of receivers
    /// </summary>
    internal static class EnvelopeDispatcher
    {
        /// <summary>
        /// Starts a new task that will receive envelopes from a producer function and dispatch them to a set of receivers
        /// </summary>
        /// <typeparam name="TEnvelope">Envelope type</typeparam>
        /// <param name="producer">Producer function</param>
        /// <param name="commandSender">Sender passed to the receivers to eventually respond to the received envelopes</param>
        /// <param name="messageSender">Sender passed to the receivers to eventually respond to the received envelopes</param>
        /// <param name="notificationSender">Sender passed to the receivers to eventually respond to the received envelopes</param>
        /// <param name="receiverFor">Function that return the receivers for the given envelope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public static Task StartAsync<TEnvelope>(Func<CancellationToken, Task<TEnvelope>> producer,
            IEnvelopeSender envelopeSender, Func<TEnvelope, IEnumerable<IEnvelopeReceiver<TEnvelope>>> receiverFor, CancellationToken cancellationToken)
            where TEnvelope : Envelope
        {
            return Task.Run(() => Process(producer, envelopeSender, receiverFor, cancellationToken), cancellationToken);
        }

        private static async Task Process<TEnvelope>(Func<CancellationToken, Task<TEnvelope>> producer,
            IEnvelopeSender envelopeSender, Func<TEnvelope, IEnumerable<IEnvelopeReceiver<TEnvelope>>> receiverFor, CancellationToken cancellationToken)
            where TEnvelope : Envelope
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TEnvelope envelope;

                try
                {
                    envelope = await producer(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    throw;
                }

                try
                {
                    var receivers = receiverFor(envelope);
                    await Task
                            .WhenAll(
                                receivers.Select(r =>
                                    CallReceiver(envelopeSender, r, envelope)))
                            .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                    Trace.TraceError(ex.ToString());                    
                }
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
