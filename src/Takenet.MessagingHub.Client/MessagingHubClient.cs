using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Lime;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Default implementation for <see cref="IMessagingHubClient"/> class.
    /// </summary>
    public class MessagingHubClient : IMessagingHubClient
    {
        public const string DEFAULT_DOMAIN = "msging.net";

        public bool Started { get; private set; }

        private readonly Uri _endpoint;
        private readonly IDictionary<MediaType, IList<Func<IMessageReceiver>>> _messageReceivers;
        private readonly IDictionary<Event, IList<Func<INotificationReceiver>>> _notificationReceivers;
        private readonly IList<Func<IMessageReceiver>> _defaultMessageReceivers = new List<Func<IMessageReceiver>> { () => new UnsupportedMessageReceiver() };
        private readonly IList<Func<INotificationReceiver>> _defaultNotificationReceivers = new List<Func<INotificationReceiver>> { () => new BlackholeNotificationReceiver() };

        private string _login;
        private string _password;
        private string _accessKey;
        private readonly string _domainName;

        private readonly IPersistentLimeSessionFactory _persistentClientFactory;
        private IPersistentLimeSession _clientChannel;

        private ICommandProcessor _commandProcessor;

        private readonly IClientChannelFactory _clientChannelFactory;
        private readonly ICommandProcessorFactory _commandProcessorFactory;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundExecution;
        private Task _messageReceiverTask;
        private Task _notiticationReceiverTask;
        private readonly TimeSpan _timeout;
        private readonly ILimeSessionProvider _limeSessionProvider;

        internal MessagingHubClient(IPersistentLimeSessionFactory persistentChannelFactory, IClientChannelFactory clientChannelFactory,
            ICommandProcessorFactory commandProcessorFactory, ILimeSessionProvider limeSessionProvider, string hostName, string domainName)
        {
            _messageReceivers = new Dictionary<MediaType, IList<Func<IMessageReceiver>>>();
            _notificationReceivers = new Dictionary<Event, IList<Func<INotificationReceiver>>>();
            _persistentClientFactory = persistentChannelFactory;
            _clientChannelFactory = clientChannelFactory;
            _commandProcessorFactory = commandProcessorFactory;
            _limeSessionProvider = limeSessionProvider;
            _domainName = domainName;
            _endpoint = new Uri($"net.tcp://{hostName}:55321");
            _timeout = TimeSpan.FromSeconds(60);
        }


        public MessagingHubClient(string hostname = DEFAULT_DOMAIN, string domainName = DEFAULT_DOMAIN) :
            this(new PersistentLimeSessionFactory(), new ClientChannelFactory(), new CommandProcessorFactory(), new LimeSessionProvider(), hostname, domainName)
        { }

        public IMessagingHubClient UsingAccount(string login, string password)
        {
            if (Started) throw new InvalidOperationException("The client is already started");

            _login = login;
            _password = password;
            return this;
        }

        public IMessagingHubClient UsingAccessKey(string login, string key)
        {
            if (Started) throw new InvalidOperationException("The client is already started");

            _login = login;
            _accessKey = key;
            return this;
        }

        public IMessagingHubClient AddMessageReceiver(IMessageReceiver messageReceiver, MediaType forMimeType = null)
        {
            if (messageReceiver == null) throw new ArgumentNullException(nameof(messageReceiver));

            return AddMessageReceiver(() => messageReceiver, forMimeType);
        }

        public IMessagingHubClient AddNotificationReceiver(INotificationReceiver notificationReceiver, Event? forEventType = null)
        {
            if (notificationReceiver == null) throw new ArgumentNullException(nameof(notificationReceiver));

            return AddNotificationReceiver(() => notificationReceiver, forEventType);
        }

        public IMessagingHubClient AddMessageReceiver(Func<IMessageReceiver> receiverFactory, MediaType forMimeType = null)
        {
            if (receiverFactory == null) throw new ArgumentNullException(nameof(receiverFactory));
            if (Started) throw new InvalidOperationException("Cannot add a receiver after the client has been started");

            var mediaTypeToSave = forMimeType ?? MediaTypes.Any;

            IList<Func<IMessageReceiver>> mediaTypeReceivers;
            if (!_messageReceivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<Func<IMessageReceiver>>();
                _messageReceivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiverFactory);
            return this;
        }

        public IMessagingHubClient AddNotificationReceiver(Func<INotificationReceiver> receiverFactory, Event? forEventType = default(Event?))
        {
            if (Started) throw new InvalidOperationException("Cannot add a receiver after the client has been started");

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

            return this;
        }

        public async Task<Command> SendCommandAsync(Command command)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation");

            return await _commandProcessor.SendAsync(command, _timeout).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(Message message)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation");

            await _clientChannel.SendMessageAsync(message).ConfigureAwait(false);
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            if (!Started) throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            await _clientChannel.SendNotificationAsync(notification).ConfigureAwait(false);
        }

        public async Task StartAsync()
        {
            InstantiateGlobalCancellationTokenSource();

            await InstantiateClientChannelAsync().ConfigureAwait(false);
            await _clientChannel.StartAsync().ConfigureAwait(false);
            await SetPresenceAsync().ConfigureAwait(false);

            StartEnvelopeProcessors();
            InitializeAndStartReceivers();

            Started = true;
        }

        public async Task StopAsync()
        {
            if (!Started)
                throw new InvalidOperationException(
                    "Client must be started before to proceed with this operation!");

            await _commandProcessor.StopReceivingAsync().ConfigureAwait(false);

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _backgroundExecution.ConfigureAwait(false);
                _cancellationTokenSource.Dispose();
            }
            await _clientChannel.StopAsync().ConfigureAwait(false);
            Started = false;
        }

        private void InstantiateGlobalCancellationTokenSource()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void StartEnvelopeProcessors()
        {
            _commandProcessor = _commandProcessorFactory.Create(_clientChannel);
            _commandProcessor.StartReceiving();
        }

        private async Task InstantiateClientChannelAsync()
        {
            var authentication = GetAuthenticationScheme();
            var identity = Identity.Parse($"{_login}@{_domainName}");

            _clientChannel = await _persistentClientFactory.CreatePersistentClientChannelAsync(_endpoint, _timeout, identity, authentication, _clientChannelFactory, _limeSessionProvider);
        }

        private async Task SetPresenceAsync()
        {
            await _clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity },
                _cancellationTokenSource.Token);
        }

        private void InitializeAndStartReceivers()
        {
            _messageReceiverTask = EnvelopeDispatcher.StartAsync(
                    _clientChannel.ReceiveMessageAsync,
                    this, this, this,
                    GetReceiversFor,
                    _cancellationTokenSource.Token);

            _notiticationReceiverTask = EnvelopeDispatcher.StartAsync(
                    _clientChannel.ReceiveNotificationAsync,
                    this, this, this,
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

        private Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (_password != null)
            {
                var plainAuthentication = new PlainAuthentication();
                plainAuthentication.SetToBase64Password(_password);
                result = plainAuthentication;
            }

            if (_accessKey != null)
            {
                var keyAuthentication = new KeyAuthentication { Key = _accessKey };
                result = keyAuthentication;
            }

            if (result == null)
                throw new InvalidOperationException($"A password or accessKey should be defined. Please use the '{nameof(UsingAccount)}' or '{nameof(UsingAccessKey)}' methods for that.");

            return result;
        }
    }
}