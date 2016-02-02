using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;
using System.Threading;
using Lime.Protocol.Security;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client
{
    public class EnvelopeListener : MessagingHubClient, IEnvelopeListener
    {
        private readonly IList<ReceiverFactoryPredicate<Message>> _messageReceivers;
        private readonly IList<ReceiverFactoryPredicate<Notification>> _notificationReceivers;

        private static readonly IEnumerable<IMessageReceiver> DefaultMessageReceivers = new IMessageReceiver[] { new UnsupportedMessageReceiver() };
        private static readonly IEnumerable<INotificationReceiver> DefaultNotificationReceivers = new INotificationReceiver[] { new BlackholeNotificationReceiver() };

        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundExecution;
        private Task _messageReceiverTask;
        private Task _notiticationReceiverTask;
        private bool _started;

        internal EnvelopeListener(Identity identity, Authentication authentication, Uri endPoint, TimeSpan sendTimeout, IPersistentLimeSessionFactory persistentChannelFactory, IClientChannelFactory clientChannelFactory,
            ICommandProcessorFactory commandProcessorFactory, ILimeSessionProvider limeSessionProvider)
            : base(identity, authentication, endPoint, sendTimeout, persistentChannelFactory, clientChannelFactory,
            commandProcessorFactory, limeSessionProvider)
        {
            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>();
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>();
        }

        public EnvelopeListener(Identity identity, Authentication authentication, Uri endPoint)
            : base(identity, authentication, endPoint)
        {
            _messageReceivers = new List<ReceiverFactoryPredicate<Message>>();
            _notificationReceivers = new List<ReceiverFactoryPredicate<Notification>>();
        }

        public EnvelopeListener(Identity identity, Authentication authentication, Uri endPoint, TimeSpan sendTimeout)
            : base(identity, authentication, endPoint, sendTimeout)
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

        public override async Task StartAsync()
        {
            await base.StartAsync().ConfigureAwait(false);

            _cancellationTokenSource = new CancellationTokenSource();
            InitializeAndStartReceivers();

            _started = true;
        }

        public async Task StopSync()
        {
            await base.StopAsync().ConfigureAwait(false);

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _backgroundExecution.ConfigureAwait(false);
                _cancellationTokenSource.Dispose();
            }

            _started = false;
        }

        private void InitializeAndStartReceivers()
        {
            _messageReceiverTask = EnvelopeDispatcher.StartAsync(
                    ReceiveMessageAsync,
                    this,
                    GetReceiversFor,
                    _cancellationTokenSource.Token);

            _notiticationReceiverTask = EnvelopeDispatcher.StartAsync(
                    ReceiveNotificationAsync,
                    this,
                    GetReceiversFor,
                    _cancellationTokenSource.Token
                    );

            _backgroundExecution = Task.WhenAll(_messageReceiverTask, _notiticationReceiverTask);
        }

        private IEnumerable<IEnvelopeReceiver<Message>> GetReceiversFor(Message message)
        {
            return GetReceiversFor(_messageReceivers, message).Coalesce(DefaultMessageReceivers);
        }

        private IEnumerable<IEnvelopeReceiver<Notification>> GetReceiversFor(Notification notification)
        {
            return GetReceiversFor(_notificationReceivers, notification).Coalesce(DefaultNotificationReceivers);
        }

        private IEnumerable<IEnvelopeReceiver<T>> GetReceiversFor<T>(
            IEnumerable<ReceiverFactoryPredicate<T>> envelopeReceivers,             
            T envelope) where T : Envelope, new()
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            return envelopeReceivers.Where(r => r.Predicate(envelope)).Select(r => r.ReceiverFactory());            
        }


        private void AddEnvelopeReceiver<T>(IList<ReceiverFactoryPredicate<T>> envelopeReceivers,
            Func<IEnvelopeReceiver<T>> receiverFactory, Predicate<T> predicate) where T : Envelope, new()
        {
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (_started) throw new InvalidOperationException("Cannot add a receiver after the client has been started");

            var predicateReceiverFactory = new ReceiverFactoryPredicate<T>(receiverFactory, predicate);
            envelopeReceivers.Add(predicateReceiverFactory);
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
