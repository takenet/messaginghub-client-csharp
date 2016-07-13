using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class EnvelopeListenerRegistrar
    {
        private readonly IMessagingHubListener _listener;
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;

        internal EnvelopeListenerRegistrar(IMessagingHubListener listener)
        {
            _listener = listener;
            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>(new[]
            {
                new ReceiverFactoryPredicate<Message>(() => new UnsupportedMessageReceiver(), m => true, int.MaxValue)
            });
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>(new[]
            {
                new ReceiverFactoryPredicate<Notification>(() => new BlackholeNotificationReceiver(), m => true, int.MaxValue)
            });
        }

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The message predicate used as a filter of messages received by listener.</param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> predicate, int priority, CancellationToken cancellationToken) => 
            AddEnvelopeReceiver(_messageReceivers, receiverFactory, predicate, priority);

        /// <summary>
        /// Add a notification receiver listener to handle received notifications.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The notification predicate used as a filter of notifications received by listener.</param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        public void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Predicate<Notification> predicate, int priority, CancellationToken cancellationToken) =>        
            AddEnvelopeReceiver(_notificationReceivers, receiverFactory, predicate, priority);
        
        public bool HasRegisteredReceivers => _messageReceivers.Any() || _notificationReceivers.Any();

        public IEnumerable<ReceiverFactoryPredicate<TEnvelope>> GetReceiversFor<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            if (envelope is Message)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>)
                    FilterReceivers(_messageReceivers, envelope as Message);
            }

            if (envelope is Notification)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>)
                    FilterReceivers(_notificationReceivers, envelope as Notification);
            }

            return Enumerable.Empty<ReceiverFactoryPredicate<TEnvelope>>();
        }

        private void AddEnvelopeReceiver<T>(
            IList<ReceiverFactoryPredicate<T>> envelopeReceivers,
            Func<IEnvelopeReceiver<T>> receiverFactory, 
            Predicate<T> predicate, 
            int priority) where T : Envelope, new()
        {
            if (_listener.Listening) throw new InvalidOperationException("Cannot add receivers when the listener is already started listening");
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate, priority);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private static IEnumerable<ReceiverFactoryPredicate<TEnvelope>> FilterReceivers<TEnvelope>(
            IEnumerable<ReceiverFactoryPredicate<TEnvelope>> envelopeReceivers, 
            TEnvelope envelope) 
            where TEnvelope : Envelope, new()
        {
            return envelopeReceivers.Where(r => r.Predicate(envelope));
        }

        internal class ReceiverFactoryPredicate<T> where T : Envelope, new()
        {
            public ReceiverFactoryPredicate(Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> predicate, int priority)
            {
                if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                ReceiverFactory = receiverFactory;
                Predicate = predicate;
                Priority = priority;
            }

            public Func<IEnvelopeReceiver<T>> ReceiverFactory { get; }

            public Predicate<T> Predicate { get; }
            public int Priority { get; set; }
        }
    }
}
