using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Dispatch envelopes from a producer to a set of receivers
    /// </summary>
    internal static class EnvelopeDispatcher
    {
        public static Task StartAsync<TEnvelope>(Func<CancellationToken, Task<TEnvelope>> producer, ISenderWrapper sender,
            Func<TEnvelope, IEnumerable<IReceiver<TEnvelope>>> receiverFor, CancellationToken cancellationToken)
            where TEnvelope : Envelope
        {
            return Task.Run(() => Process(producer, sender, receiverFor, cancellationToken), cancellationToken);
        }

        private static async Task Process<TEnvelope>(Func<CancellationToken, Task<TEnvelope>> producer, ISenderWrapper sender, 
            Func<TEnvelope, IEnumerable<IReceiver<TEnvelope>>> receiverFor, CancellationToken cancellationToken)
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
                    await Task.WhenAll(receivers.Select(r => CallReceiver(sender, r, envelope))).ConfigureAwait(false);
                }
                catch
                {
                    //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                }
            }
        }

        private static Task CallReceiver<TEnvelope>(ISenderWrapper sender, IReceiver<TEnvelope> receiver, TEnvelope envelope)
            where TEnvelope : Envelope
        {
            InjectSenders(sender, receiver);
            return receiver.ReceiveAsync(envelope);
        }

        private static void InjectSenders(ISenderWrapper sender, object receiver)
        {
            if (!(receiver is ReceiverBase))
                return;

            var receiverBase = ((ReceiverBase)receiver);
            receiverBase.MessageSender = sender;
            receiverBase.CommandSender = sender;
            receiverBase.NotificationSender = sender;
        }
    }
}
