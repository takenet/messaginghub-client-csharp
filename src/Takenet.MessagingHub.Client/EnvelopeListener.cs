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
        private readonly IDictionary<MediaType, IList<Func<IMessageReceiver>>> _messageReceivers;
        private readonly IDictionary<Event, IList<Func<INotificationReceiver>>> _notificationReceivers;
        private readonly IList<Func<IMessageReceiver>> _defaultMessageReceivers = new List<Func<IMessageReceiver>> { () => new UnsupportedMessageReceiver() };
        private readonly IList<Func<INotificationReceiver>> _defaultNotificationReceivers = new List<Func<INotificationReceiver>> { () => new BlackholeNotificationReceiver() };
        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundExecution;
        private Task _messageReceiverTask;
        private Task _notiticationReceiverTask;
        private bool _started;

        internal EnvelopeListener(string login, Authentication authentication, Uri endPoint, string domainName, IPersistentLimeSessionFactory persistentChannelFactory, IClientChannelFactory clientChannelFactory,
            ICommandProcessorFactory commandProcessorFactory, ILimeSessionProvider limeSessionProvider)
            : base(login, authentication, endPoint, domainName, persistentChannelFactory, clientChannelFactory,
            commandProcessorFactory, limeSessionProvider)
        {
            _messageReceivers = new Dictionary<MediaType, IList<Func<IMessageReceiver>>>();
            _notificationReceivers = new Dictionary<Event, IList<Func<INotificationReceiver>>>();
        }

        public EnvelopeListener(string login, Authentication authentication, Uri endPoint, string domainName)
            : base(login, authentication, endPoint, domainName)
        {
            _messageReceivers = new Dictionary<MediaType, IList<Func<IMessageReceiver>>>();
            _notificationReceivers = new Dictionary<Event, IList<Func<INotificationReceiver>>>();
        }
        
        public void AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (_started) throw new InvalidOperationException("Cannot add a receiver after the client has been started");

            var mediaTypeToSave = forMimeType ?? MediaTypes.Any;

            IList<Func<IMessageReceiver>> mediaTypeReceivers;
            if (!_messageReceivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<Func<IMessageReceiver>>();
                _messageReceivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiverFactory);
        }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            if (messageReceiver == null) throw new ArgumentNullException(nameof(messageReceiver));

            AddMessageReceiver(() => messageReceiver, forMimeType);
        }

        public void AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = default(Event?))
        {
            if (_started) throw new InvalidOperationException("Cannot add a receiver after the client has been started");

            IList<Func<INotificationReceiver>> eventTypeReceivers;

            if (forEventType.HasValue)
            {
                if (!_notificationReceivers.TryGetValue(forEventType.Value, out eventTypeReceivers))
                {
                    eventTypeReceivers = new List<Func<INotificationReceiver>>();
                    _notificationReceivers.Add(forEventType.Value, eventTypeReceivers);
                }

                eventTypeReceivers.Add(receiverFactory);
            }
            else
            {
                eventTypeReceivers = new List<Func<INotificationReceiver>> { receiverFactory };

                _notificationReceivers.Add(Event.Accepted, eventTypeReceivers);
                _notificationReceivers.Add(Event.Authorized, eventTypeReceivers);
                _notificationReceivers.Add(Event.Consumed, eventTypeReceivers);
                _notificationReceivers.Add(Event.Dispatched, eventTypeReceivers);
                _notificationReceivers.Add(Event.Failed, eventTypeReceivers);
                _notificationReceivers.Add(Event.Received, eventTypeReceivers);
                _notificationReceivers.Add(Event.Validated, eventTypeReceivers);
            }
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = default(Event?))
        {
            if (notificationReceiver == null) throw new ArgumentNullException(nameof(notificationReceiver));

            AddNotificationReceiver(() => notificationReceiver, forEventType);
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();

            _cancellationTokenSource = new CancellationTokenSource();

            InitializeAndStartReceivers();

            _started = true;
        }

        public async Task StopSync()
        {
            await base.StopAsync();

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

        private IEnumerable<IMessageReceiver> GetReceiversFor(Message message)
        {
            IList<Func<IMessageReceiver>> mimeTypeReceiversFunc;

            var hasReceiver = _messageReceivers.TryGetValue(message.Type, out mimeTypeReceiversFunc) ||
                              _messageReceivers.TryGetValue(MediaTypes.Any, out mimeTypeReceiversFunc);

            if (!hasReceiver)
                mimeTypeReceiversFunc = _defaultMessageReceivers;

            return mimeTypeReceiversFunc.Select(m => m?.Invoke()).ToList();
        }

        private IEnumerable<INotificationReceiver> GetReceiversFor(Notification notificaiton)
        {
            IList<Func<INotificationReceiver>> eventTypeReceiversFunc;
            var hasReceiver = _notificationReceivers.TryGetValue(notificaiton.Event, out eventTypeReceiversFunc);
            if (!hasReceiver)
                eventTypeReceiversFunc = _defaultNotificationReceivers;

            return eventTypeReceiversFunc.Select(m => m?.Invoke()).ToList();
        }

        
    }
}
