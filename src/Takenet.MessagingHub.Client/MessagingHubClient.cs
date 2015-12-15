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
        static readonly string defaultDomainName = "msging.net";

        public IMessageSender MessageSender => sender;
        public ICommandSender CommandSender => sender;
        public INotificationSender NotificationSender => sender;

        readonly Uri _endpoint;
        readonly IDictionary<MediaType, IList<Func<IMessageReceiver>>> _messageReceivers;
        readonly IDictionary<Event, IList<Func<INotificationReceiver>>> _notificationReceivers;

        readonly IList<Func<IMessageReceiver>> _defaultMessageReceivers = new List<Func<IMessageReceiver>> { () => new UnsupportedMessageReceiver() };
        readonly IList<Func<INotificationReceiver>> _defaultNotificationReceivers = new List<Func<INotificationReceiver>> { () => new BlackholeNotificationReceiver() };

        string _login;
        string _password;
        string _accessKey;
        string _domainName;
        IClientChannel _clientChannel;
        IClientChannelFactory _clientChannelFactory;
        IEnvelopeProcessorFactory<Command> _envelopeProcessorFactory;
        IEnvelopeProcessor<Command> _commandProcessor;
        ISessionFactory _sessionFactory;
        SenderWrapper sender;

        CancellationTokenSource _cancellationTokenSource;
        Task _backgroundExecution;
        Task _messageReceiver;
        Task _notiticationReceiver;

        TimeSpan _timeout;
        internal MessagingHubClient(IClientChannelFactory clientChannelFactory, ISessionFactory sessionFactory, IEnvelopeProcessorFactory<Command> envelopeProcessorFactory, string hostname = null, string domainName = null)
        {
            _messageReceivers = new Dictionary<MediaType, IList<Func<IMessageReceiver>>>();
            _notificationReceivers = new Dictionary<Event, IList<Func<INotificationReceiver>>>();
            _clientChannelFactory = clientChannelFactory;
            _envelopeProcessorFactory = envelopeProcessorFactory;
            _sessionFactory = sessionFactory;
            hostname = hostname ?? defaultDomainName;
            _endpoint = new Uri($"net.tcp://{hostname}:55321");
            _domainName = domainName ?? defaultDomainName;
            _timeout = TimeSpan.FromSeconds(60);
        }

        public MessagingHubClient(string hostname = null, string domainName = null) :
            this(new ClientChannelFactory(), new SessionFactory(), new CommandProcessorFactory(), hostname, domainName)
        { }

        public MessagingHubClient UsingAccount(string login, string password)
        {
            this._login = login;
            this._password = password;
            return this;
        }

        public MessagingHubClient UsingAccessKey(string login, string key)
        {
            this._login = login;
            this._accessKey = key;
            return this;
        }

        #region PublicMethods

        /// <summary>
        /// Add a message receiver listener to handle received messages
        /// </summary>
        /// <param name="receiver">Listener</param>
        /// <param name="forMimeType">MediaType used as a filter of messages received by listener. When not informed, only receives messages which no 'typed' receiver is registered</param>
        /// <returns></returns>
        public MessagingHubClient AddMessageReceiver(IMessageReceiver receiver, MediaType forMimeType = null)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            return AddMessageReceiver(() => { return receiver; }, forMimeType);
        }

        /// <summary>
        /// Add a notification receiver listener to handle received notifications
        /// </summary>
        /// <param name="receiver">Listener</param>
        /// <param name="forMimeType">MediaType used as a filter of notification received by listener. When not informed, only receives notifications which no 'typed' receiver is registered</param>
        /// <returns></returns>
        public MessagingHubClient AddNotificationReceiver(INotificationReceiver receiver, Event? forEventType = null)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            return AddNotificationReceiver(() => { return receiver; }, forEventType);
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
                eventTypeReceivers = new List<Func<INotificationReceiver>>();
                eventTypeReceivers.Add(receiverBuild);

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

        /// <summary>
        /// Connect and receives messages from Lime server
        /// </summary>
        /// <returns>Task representing the running state of the client (when this tasks finishes, the connection has been terminated)</returns>
        public async Task StartAsync()
        {
            var authentication = GetAuthenticationScheme();

            _clientChannel = await _clientChannelFactory.CreateClientChannelAsync(_endpoint).ConfigureAwait(false);

            var identity = Identity.Parse($"{_login}@{_domainName}");
            var session = await _sessionFactory.CreateSessionAsync(_clientChannel, identity, authentication).ConfigureAwait(false);

            if (session.State != SessionState.Established)
            {
                throw new LimeException(session.Reason.Code, session.Reason.Description);
            }
            
            await _clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { RoutingRule = RoutingRule.Identity },
                CancellationToken.None)
                .ConfigureAwait(false);

            _commandProcessor = _envelopeProcessorFactory.Create(_clientChannel);
            _commandProcessor.Start();

            sender = new SenderWrapper(_clientChannel, _commandProcessor, _timeout);
            _cancellationTokenSource = new CancellationTokenSource();
            InitializeAndStartReceivers();
            _backgroundExecution = Task.WhenAll(_messageReceiver, _notiticationReceiver);
        }

        /// <summary>
        /// Close connecetion and stop to receive messages from Lime server 
        /// </summary>
        /// <returns>
        public async Task StopAsync()
        {
            if (_clientChannel == null) throw new InvalidOperationException("Is not possible call 'Stop' method before 'Start'");

            await _commandProcessor.StopAsync();

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

        #endregion PublicMethods

        #region InternalMethods

        void InitializeAndStartReceivers()
        {
            _messageReceiver = new EnvelopeProcessor<Message>(
                    _clientChannel.ReceiveMessageAsync,
                    GetReceiversFor,
                    sender)
                .StartAsync(_cancellationTokenSource.Token);

            _notiticationReceiver = new EnvelopeProcessor<Notification>(
                    _clientChannel.ReceiveNotificationAsync,
                    GetReceiversFor,
                    sender)
                .StartAsync(_cancellationTokenSource.Token);
        }

        IList<IMessageReceiver> GetReceiversFor(Message message)
        {
            IList<Func<IMessageReceiver>> mimeTypeReceiversFunc = null;
            var hasReceiver = _messageReceivers.TryGetValue(message.Type, out mimeTypeReceiversFunc) ||
                              _messageReceivers.TryGetValue(MediaTypes.Any, out mimeTypeReceiversFunc);
            if (!hasReceiver)
            {
                mimeTypeReceiversFunc = _defaultMessageReceivers;
            }

            var mimeTypeReceivers = new List<IMessageReceiver>();

            foreach(var m in mimeTypeReceiversFunc)
            {
                mimeTypeReceivers.Add(m?.Invoke());
            }

            return mimeTypeReceivers;
        }
        
        IList<INotificationReceiver> GetReceiversFor(Notification notificaiton)
        {
            var notificationReceivers = new List<INotificationReceiver>();

            foreach (var m in _defaultNotificationReceivers)
            {
                notificationReceivers.Add(m?.Invoke());
            }

            return notificationReceivers;
        }

        Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (this._password != null)
            {
                var plainAuthentication = new PlainAuthentication();
                plainAuthentication.SetToBase64Password(this._password);
                result = plainAuthentication;
            }

            if (this._accessKey != null)
            {
                var keyAuthentication = new KeyAuthentication { Key = _accessKey };
                result = keyAuthentication;
            }

            if (result == null)
            {
                throw new InvalidOperationException($"A password (method {nameof(UsingAccount)}) or accessKey (method {nameof(UsingAccessKey)}) should be informed");
            }

            return result;
        }

        #endregion InternalMethods
    }
}