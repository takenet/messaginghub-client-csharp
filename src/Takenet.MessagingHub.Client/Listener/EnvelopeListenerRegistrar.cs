using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class EnvelopeListenerRegistrar
    {
        private readonly IMessagingHubListener _listener;
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;

        private static readonly IEnumerable<IMessageReceiver> DefaultMessageReceivers = new IMessageReceiver[] { new UnsupportedMessageReceiver() };
        private static readonly IEnumerable<INotificationReceiver> DefaultNotificationReceivers = new INotificationReceiver[] { new BlackholeNotificationReceiver() };

        internal EnvelopeListenerRegistrar(IMessagingHubListener listener)
        {
            _listener = listener;
            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>();
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>();
        }

        /// <summary>
        /// Add a message receiver listener to handle received messages.
        /// </summary>
        /// <param name="receiverFactory">A function used to build the notification listener</param>
        /// <param name="predicate">The message predicate used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered.</param>
        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> predicate)
        {
            AddEnvelopeReceiver(_messageReceivers, receiverFactory, predicate);
        }

        ///// <summary>
        ///// Add a notification receiver listener to handle received notifications.
        ///// </summary>
        ///// <param name="receiverFactory">A function used to build the notification listener</param>
        ///// <param name="forEventType">EventType used as a filter of notification received by listener.</param>
        ///// <returns></returns>
        public void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Predicate<Notification> predicate)
        {
            AddEnvelopeReceiver(_notificationReceivers, receiverFactory, predicate);
        }

        public bool HasRegisteredReceivers => _messageReceivers.Any() || _notificationReceivers.Any();

        public IEnumerable<IEnvelopeReceiver<TEnvelope>> GetReceiversFor<TEnvelope>(TEnvelope envelope)
            where TEnvelope : Envelope
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            if (envelope is Message)
            {
                return (IEnumerable<IEnvelopeReceiver<TEnvelope>>) GetReceiversFor(_messageReceivers, envelope as Message).Coalesce(DefaultMessageReceivers);
            }

            if (envelope is Notification)
            {
                return (IEnumerable<IEnvelopeReceiver<TEnvelope>>) GetReceiversFor(_notificationReceivers, envelope as Notification).Coalesce(DefaultNotificationReceivers);
            }

            return Enumerable.Empty<IEnvelopeReceiver<TEnvelope>>();
        }

        private void AddEnvelopeReceiver<T>(IList<ReceiverFactoryPredicate<T>> envelopeReceivers,
            Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> predicate) where T : Envelope, new()
        {
            if (_listener.Listening) throw new InvalidOperationException("Cannot add receivers when the listener is already started listening");

            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private static IEnumerable<IEnvelopeReceiver<TEnvelope>> GetReceiversFor<TEnvelope>(
            IEnumerable<ReceiverFactoryPredicate<TEnvelope>> envelopeReceivers, TEnvelope envelope) 
            where TEnvelope : Envelope, new()
        {
            return envelopeReceivers.Where(r => r.EnvelopeFilter(envelope)).Select(r => r.ReceiverFactory());
        }
        
        class ReceiverFactoryPredicate<T> where T : Envelope, new()
        {
            public ReceiverFactoryPredicate(Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> envelopeFilter)
            {
                if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
                if (envelopeFilter == null) throw new ArgumentNullException(nameof(envelopeFilter));

                ReceiverFactory = receiverFactory;
                EnvelopeFilter = envelopeFilter;
            }

            public Func<IEnvelopeReceiver<T>> ReceiverFactory { get; }

            public Predicate<T> EnvelopeFilter { get; }
        }
    }
}
