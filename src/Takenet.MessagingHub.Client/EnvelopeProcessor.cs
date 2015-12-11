using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client
{
    class EnvelopeProcessor<T>
        where T : Envelope
    {
        readonly SenderWrapper sender;
        readonly Func<CancellationToken, Task<T>> producer;
        readonly Func<T, IEnumerable<IReceiver<T>>> getReceivers;

        public EnvelopeProcessor(Func<CancellationToken, Task<T>> producer, Func<T, IEnumerable<IReceiver<T>>> getReceivers, SenderWrapper sender)
        {
            this.producer = producer;
            this.getReceivers = getReceivers;
            this.sender = sender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => Process(cancellationToken));
        }

        async Task Process(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var envelope = await producer(cancellationToken).ConfigureAwait(false);

                //When cancelation 
                if (envelope == null)
                {
                    continue;
                }

                try
                {
                    var receivers = getReceivers(envelope);
                    await Task.WhenAll(
                                receivers.Select(r => CallReceiver(r, envelope))).ConfigureAwait(false);
                }
                catch { }
            }
        }

        Task CallReceiver(IReceiver<T> receiver, T envelope)
        {
            InjectSenders(receiver);
            return receiver.ReceiveAsync(envelope);
        }

        void InjectSenders(object receiver)
        {
            if (receiver is ReceiverBase)
            {
                var receiverBase = ((ReceiverBase)receiver);
                receiverBase.MessageSender = sender;
                receiverBase.CommandSender = sender;
                receiverBase.NotificationSender = sender;
            }
        }

    }
}
