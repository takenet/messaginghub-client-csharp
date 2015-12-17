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
    public class MessagingHubClient : IMessagingHubClient
    {
        private const string DefaultDomainName = "msging.net";

        private readonly Uri _endpoint;
        private readonly IDictionary<MediaType, IList<Func<IMessageReceiver>>> _messageReceivers;
        private readonly IDictionary<Event, IList<Func<INotificationReceiver>>> _notificationReceivers;

        private readonly IList<Func<IMessageReceiver>> _defaultMessageReceivers = new List<Func<IMessageReceiver>> { () => new UnsupportedMessageReceiver() };
        private readonly IList<Func<INotificationReceiver>> _defaultNotificationReceivers = new List<Func<INotificationReceiver>> { () => new BlackholeNotificationReceiver() };

        private string _login;
        private string _password;
        private string _accessKey;
        private readonly string _domainName;

        private IClientChannel _clientChannel;

        private IEnvelopeProcessor<Command> _commandProcessor;
        private IEnvelopeProcessor<Message> _messageProcessor;
        private IEnvelopeProcessor<Notification> _notificationProcessor;

        private CommandSender _commandSender;
        private MessageSender _messageSender;
        private NotificationSender _notificationSender;

        private readonly IClientChannelFactory _clientChannelFactory;
        private readonly ISessionFactory _sessionFactory;

        private readonly IEnvelopeProcessorFactory<Command> _commandProcessorFactory;
        private readonly IEnvelopeProcessorFactory<Message> _messageProcessorFactory;
        private readonly IEnvelopeProcessorFactory<Notification> _notificationProcessorFactory;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundExecution;
        private Task _messageReceiverTask;
        private Task _notiticationReceiverTask;
        private readonly TimeSpan _timeout;


        internal MessagingHubClient(IClientChannelFactory clientChannelFactory, ISessionFactory sessionFactory,
            IEnvelopeProcessorFactory<Command> commandProcessorFactory, IEnvelopeProcessorFactory<Message> messageProcessorFactory, IEnvelopeProcessorFactory<Notification> notificationProcessorFactory, string hostname = null, string domainName = null)
        {
            _messageReceivers = new Dictionary<MediaType, IList<Func<IMessageReceiver>>>();
            _notificationReceivers = new Dictionary<Event, IList<Func<INotificationReceiver>>>();
            _clientChannelFactory = clientChannelFactory;
            _commandProcessorFactory = commandProcessorFactory;
            _messageProcessorFactory = messageProcessorFactory;
            _notificationProcessorFactory = notificationProcessorFactory;
            _sessionFactory = sessionFactory;
            hostname = hostname ?? DefaultDomainName;
            _endpoint = new Uri($"net.tcp://{hostname}:55321");
            _domainName = domainName ?? DefaultDomainName;
            _timeout = TimeSpan.FromSeconds(60);
        }


        public MessagingHubClient(string hostname = null, string domainName = null) :
                    this(new ClientChannelFactory(), new SessionFactory(), new CommandProcessorFactory(), new MessageProcessorFactory(), new NotificationProcessorFactory(), hostname, domainName)
        { }

        public MessagingHubClient UsingAccount(string login, string password)
        {
            _login = login;
            _password = password;
            return this;
        }

        public MessagingHubClient UsingAccessKey(string login, string key)
        {
            _login = login;
            _accessKey = key;
            return this;
        }

        public MessagingHubClient AddMessageReceiver(IMessageReceiver envelopeReceiver, MediaType forMimeType = null)
        {
            if (envelopeReceiver == null) throw new ArgumentNullException(nameof(envelopeReceiver));

            return AddMessageReceiver(() => envelopeReceiver, forMimeType);
        }

        public MessagingHubClient AddNotificationReceiver(INotificationReceiver envelopeReceiver, Event? forEventType = null)
        {
            if (envelopeReceiver == null) throw new ArgumentNullException(nameof(envelopeReceiver));

            return AddNotificationReceiver(() => envelopeReceiver, forEventType);
        }

        public MessagingHubClient AddMessageReceiver(Func<IMessageReceiver> receiverBuild, MediaType forMimeType = null)
        {
            if (receiverBuild == null) throw new ArgumentNullException(nameof(receiverBuild));

            var mediaTypeToSave = forMimeType ?? MediaTypes.Any;

            IList<Func<IMessageReceiver>> mediaTypeReceivers;
            if (!_messageReceivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<Func<IMessageReceiver>>();
                _messageReceivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiverBuild);
            return this;
        }

        public MessagingHubClient AddNotificationReceiver(Func<INotificationReceiver> receiverBuild, Event? forEventType = default(Event?))
        {
            IList<Func<INotificationReceiver>> eventTypeReceivers;

            if (forEventType.HasValue)
            {
                if (!_notificationReceivers.TryGetValue(forEventType.Value, out eventTypeReceivers))
                {
                    eventTypeReceivers = new List<Func<INotificationReceiver>>();
                    _notificationReceivers.Add(forEventType.Value, eventTypeReceivers);
                }

                eventTypeReceivers.Add(receiverBuild);
            }
            else
            {
                eventTypeReceivers = new List<Func<INotificationReceiver>> { receiverBuild };

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
            if (_commandSender == null) throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            return await _commandSender.SendAsync(command);
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            if (_messageSender == null) throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            return await _messageSender.SendAsync(message);
        }

        public async Task<Notification> SendNotificationAsync(Notification notification)
        {
            if (_notificationSender == null) throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            return await _notificationSender.SendAsync(notification);
        }

        public async Task StartAsync()
        {
            await InstanciateClientChannelAsync();

            await StablishSessionAsync();

            await SetPresenceAsync().ConfigureAwait(false);

            StartEnvelopeProcessors();

            InstanciateSenders();

            InstanciateGlobalCancellationTokenSource();

            InitializeAndStartReceivers();
        }

        public async Task StopAsync()
        {
            if (_clientChannel == null) throw new InvalidOperationException("Is not possible call 'Stop' method before 'Start'");

            await _commandProcessor.StopReceivingAsync();

            if (_clientChannel?.State == SessionState.Established)
            {
                await _clientChannel.SendFinishingSessionAsync().ConfigureAwait(false);
            }
            else
            {
                await _clientChannel.Transport.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            }

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                await _backgroundExecution;
                _cancellationTokenSource.Dispose();
            }
        }


        private void InstanciateGlobalCancellationTokenSource()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void InstanciateSenders()
        {
            _commandSender = new CommandSender(_commandProcessor, _timeout);
            _messageSender = new MessageSender(_messageProcessor, _timeout);
            _notificationSender = new NotificationSender(_notificationProcessor, _timeout);
        }


        private void StartEnvelopeProcessors()
        {
            _commandProcessor = _commandProcessorFactory.Create(_clientChannel);
            _commandProcessor.StartReceiving();

            _messageProcessor = _messageProcessorFactory.Create(_clientChannel);
            _messageProcessor.StartReceiving();

            _notificationProcessor = _notificationProcessorFactory.Create(_clientChannel);
            _notificationProcessor.StartReceiving();
        }

        private async Task InstanciateClientChannelAsync()
        {
            _clientChannel = await _clientChannelFactory.CreateClientChannelAsync(_endpoint).ConfigureAwait(false);
        }

        private async Task StablishSessionAsync()
        {
            var authentication = GetAuthenticationScheme();
            var identity = Identity.Parse($"{_login}@{_domainName}");
            var session = await _sessionFactory.CreateSessionAsync(_clientChannel, identity, authentication).ConfigureAwait(false);

            if (session.State != SessionState.Established)
            {
                throw new LimeException(session.Reason.Code, session.Reason.Description);
            }
        }

        private async Task SetPresenceAsync()
        {
            await _clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { RoutingRule = RoutingRule.Identity },
                CancellationToken.None);
        }

        private void InitializeAndStartReceivers()
        {
            _messageReceiverTask = EnvelopeDispatcher.StartAsync(
                    _clientChannel.ReceiveMessageAsync,
                    _commandSender, _messageSender, _notificationSender,
                    GetReceiversFor,
                    _cancellationTokenSource.Token);

            _notiticationReceiverTask = EnvelopeDispatcher.StartAsync(
                    _clientChannel.ReceiveNotificationAsync,
                    _commandSender, _messageSender, _notificationSender,
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
                throw new InvalidOperationException($"A password (method {nameof(UsingAccount)}) or accessKey (method {nameof(UsingAccessKey)}) should be informed");

            return result;
        }
    }
}