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
        private readonly IDictionary<int, IList<ReceiverFactoryPredicate<Message>>> _messageReceiversDictionary;
        private readonly IDictionary<int, IList<ReceiverFactoryPredicate<Notification>>> _notificationReceiversDictionary;
        private readonly object _syncRoot;

        private static readonly IEnumerable<ReceiverFactoryPredicate<Message>> DefaultMessageReceivers = new[]
        {
            new ReceiverFactoryPredicate<Message>(() => new UnsupportedMessageReceiver(), m => true, CancellationToken.None)
        };
        private static readonly IEnumerable<ReceiverFactoryPredicate<Notification>> DefaultNotificationReceivers = new []
        {
            new ReceiverFactoryPredicate<Notification>(() => new BlackholeNotificationReceiver(), m => true, CancellationToken.None)
        };

        internal EnvelopeListenerRegistrar(IMessagingHubListener listener)
        {
            _listener = listener;
            _messageReceiversDictionary = new Dictionary<int, IList<ReceiverFactoryPredicate<Message>>>();
            _notificationReceiversDictionary = new Dictionary<int, IList<ReceiverFactoryPredicate<Notification>>>();
            _syncRoot = new object();
        }

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The message predicate used as a filter of messages received by listener.</param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> predicate, int priority, CancellationToken cancellationToken) => 
            AddEnvelopeReceiver(_messageReceiversDictionary, receiverFactory, predicate, priority, cancellationToken);

        /// <summary>
        /// Add a notification receiver listener to handle received notifications.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The notification predicate used as a filter of notifications received by listener.</param>
        /// <param name="priority"></param>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        public void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Predicate<Notification> predicate, int priority, CancellationToken cancellationToken) =>        
            AddEnvelopeReceiver(_notificationReceiversDictionary, receiverFactory, predicate, priority, cancellationToken);
        
        public bool HasRegisteredReceivers => _messageReceiversDictionary.Any() || _notificationReceiversDictionary.Any();

        public IEnumerable<ReceiverFactoryPredicate<TEnvelope>> GetReceiversFor<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope, new()
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            if (envelope is Message)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>) 
                    GetReceiversFor(_messageReceiversDictionary, envelope as Message)
                        .Coalesce(DefaultMessageReceivers);
            }

            if (envelope is Notification)
            {
                return (IEnumerable<ReceiverFactoryPredicate<TEnvelope>>) 
                    GetReceiversFor(_notificationReceiversDictionary, envelope as Notification)
                        .Coalesce(DefaultNotificationReceivers);
            }

            return Enumerable.Empty<ReceiverFactoryPredicate<TEnvelope>>();
        }

        private void AddEnvelopeReceiver<T>(
            IDictionary<int, IList<ReceiverFactoryPredicate<T>>> envelopeReceiversDictionary,
            Func<IEnvelopeReceiver<T>> receiverFactory, 
            Predicate<T> predicate, 
            int priority, 
            CancellationToken cancellationToken) where T : Envelope, new()
        {
            if (_listener.Listening) throw new InvalidOperationException("Cannot add receivers when the listener is already started listening");
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            IList<ReceiverFactoryPredicate<T>> envelopeReceivers;

            lock (_syncRoot)
            {
                if (!envelopeReceiversDictionary.TryGetValue(priority, out envelopeReceivers))
                {
                    envelopeReceivers = new List<ReceiverFactoryPredicate<T>>();
                    envelopeReceiversDictionary.Add(priority, envelopeReceivers);
                }
            }

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate, cancellationToken);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private static IEnumerable<ReceiverFactoryPredicate<TEnvelope>> GetReceiversFor<TEnvelope>(
            IEnumerable<ReceiverFactoryPredicate<TEnvelope>> envelopeReceivers, TEnvelope envelope) 
            where TEnvelope : Envelope, new()
        {
            return envelopeReceivers.Where(r => r.EnvelopeFilter(envelope));
        }

        internal class ReceiverFactoryPredicate<T> where T : Envelope, new()
        {
            public ReceiverFactoryPredicate(Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> envelopeFilter, CancellationToken cancellationToken)
            {
                if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
                if (envelopeFilter == null) throw new ArgumentNullException(nameof(envelopeFilter));

                ReceiverFactory = receiverFactory;
                EnvelopeFilter = envelopeFilter;
                CancellationToken = cancellationToken;
            }

            public Func<IEnvelopeReceiver<T>> ReceiverFactory { get; }

            public Predicate<T> EnvelopeFilter { get; }

            public CancellationToken CancellationToken { get; set; }
        }
    }
}
