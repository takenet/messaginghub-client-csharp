using System;
using System.Collections.Generic;
using System.Linq;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Deprecated.Receivers;

namespace Takenet.MessagingHub.Client.Deprecated
{
    [Obsolete]
    public class EnvelopeListenerRegistrar : IEnvelopeListener
    {
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;

        private static readonly IEnumerable<IMessageReceiver> DefaultMessageReceivers = new IMessageReceiver[] { new UnsupportedMessageReceiver() };
        private static readonly IEnumerable<INotificationReceiver> DefaultNotificationReceivers = new INotificationReceiver[] { new BlackholeNotificationReceiver() };

        public EnvelopeListenerRegistrar()
        {
            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>();
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>();
        }

        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, Predicate<Message> predicate)
        {
            AddEnvelopeReceiver(_messageReceivers, receiverFactory, predicate);
        }

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

        private static void AddEnvelopeReceiver<T>(IList<ReceiverFactoryPredicate<T>> envelopeReceivers,
            Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> predicate) where T : Envelope, new()
        {
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate);
            envelopeReceivers.Add(predicateReceiverFactory);
        }

        private static IEnumerable<IEnvelopeReceiver<TEnvelope>> GetReceiversFor<TEnvelope>(
            IEnumerable<ReceiverFactoryPredicate<TEnvelope>> envelopeReceivers, TEnvelope envelope) 
            where TEnvelope : Envelope, new()
        {
            return envelopeReceivers.Where(r => r.Predicate(envelope)).Select(r => r.ReceiverFactory());
        }
        
        class ReceiverFactoryPredicate<T> where T : Envelope, new()
        {
            public ReceiverFactoryPredicate(Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> predicate)
            {
                if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
                if (predicate == null) throw new ArgumentNullException(nameof(predicate));

                ReceiverFactory = receiverFactory;
                Predicate = predicate;
            }

            public Func<IEnvelopeReceiver<T>> ReceiverFactory { get; }

            public Predicate<T> Predicate { get; }
        }
    }
}
