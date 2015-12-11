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

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClient : IMessagingHubClient
    {
        static readonly string defaultDomainName = "msging.net";

        public IMessageSender MessageSender => sender;
        public INotificationSender NotificationSender => sender;

        readonly Uri _endpoint;
        readonly IDictionary<MediaType, IList<IMessageReceiver>> _messageReceivers;
        readonly IList<IMessageReceiver> _defaultMessageReceivers = new List<IMessageReceiver> { new UnsupportedMessageReceiver() };
        readonly IList<ICommandReceiver> _defaultCommandReceivers = new List<ICommandReceiver> { new UnsupportedCommandReceiver() };
        readonly IList<INotificationReceiver> _defaultNotificationReceivers = new List<INotificationReceiver> { new BlackholeNotificationReceiver() };

        string _login;
        string _password;
        string _accessKey;
        string _domainName;
        IClientChannel _clientChannel;
        IClientChannelFactory _clientChannelFactory;
        ISessionFactory _sessionFactory;
        SenderWrapper sender;

        CancellationTokenSource _cancellationTokenSource;
        Task _backgroundExecution;
        Task _messageReceiver;
        Task _commandReceiver;
        Task _notiticationReceiver;

        internal MessagingHubClient(IClientChannelFactory clientChannelFactory, ISessionFactory sessionFactory, string hostname = null, string domainName = null)
        {
            _messageReceivers = new Dictionary<MediaType, IList<IMessageReceiver>>();
            _clientChannelFactory = clientChannelFactory;
            _sessionFactory = sessionFactory;
            hostname = hostname ?? defaultDomainName;
            _endpoint = new Uri($"net.tcp://{hostname}:55321");
            _domainName = domainName ?? defaultDomainName;
        }

        public MessagingHubClient(string hostname = null, string domainName = null) :
            this(new ClientChannelFactory(), new SessionFactory(), hostname, domainName)
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
            AddReceiver(forMimeType, receiver);
            return this;
        }

        /// <summary>
        /// Connect and receives messages from Lime server
        /// </summary>
        /// <returns>Task representing the running state of the client (when this tasks finishes, the connection has been terminated)</returns>
        public async Task<Task> StartAsync()
        {
            var authentication = GetAuthenticationScheme();

            _clientChannel = await _clientChannelFactory.CreateClientChannelAsync(_endpoint).ConfigureAwait(false);

            var identity = Identity.Parse($"{_login}@{_domainName}");
            var session = await _sessionFactory.CreateSessionAsync(_clientChannel, identity, authentication).ConfigureAwait(false);

            if (session.State != SessionState.Established)
            {
                throw new LimeException(session.Reason.Code, session.Reason.Description);
            }

            sender = new SenderWrapper(_clientChannel);
            await _clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { RoutingRule = RoutingRule.Identity },
                CancellationToken.None)
                .ConfigureAwait(false);

            _cancellationTokenSource = new CancellationTokenSource();
            _messageReceiver = Task.Run(() => ProcessIncomingMessages(_cancellationTokenSource.Token));
            _commandReceiver = Task.Run(() => ProcessIncomingCommands(_cancellationTokenSource.Token));
            _notiticationReceiver = Task.Run(() => ProcessIncomingNotifications(_cancellationTokenSource.Token));
            _backgroundExecution = Task.WhenAll(_messageReceiver, _commandReceiver, _notiticationReceiver);

            return _backgroundExecution;
        }

        /// <summary>
        /// Close connecetion and stop to receive messages from Lime server 
        /// </summary>
        /// <returns>
        public async Task StopAsync()
        {
            if (_clientChannel == null) throw new InvalidOperationException("Is not possible call 'Stop' method before 'Start'");

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

        void AddReceiver(MediaType mediaType, IMessageReceiver receiver)
        {
            var mediaTypeToSave = mediaType ?? MediaTypes.Any;

            IList<IMessageReceiver> mediaTypeReceivers;
            if (!_messageReceivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<IMessageReceiver>();
                _messageReceivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiver);
        }

        async Task ProcessIncomingMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _clientChannel.ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);

                //When cancelation 
                if (message == null)
                {
                    continue;
                }

                IList<IMessageReceiver> mimeTypeReceivers = null;
                var hasReceiver = _messageReceivers.TryGetValue(message.Type, out mimeTypeReceivers) ||
                                  _messageReceivers.TryGetValue(MediaTypes.Any, out mimeTypeReceivers);
                if (!hasReceiver)
                {
                    mimeTypeReceivers = _defaultMessageReceivers;
                }

                try
                {
                    await Task.WhenAll(
                                mimeTypeReceivers.Select(r => CallMessageReceiver(r, message))).ConfigureAwait(false);
                }
                catch { }
            }
        }

        async Task ProcessIncomingCommands(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var command = await _clientChannel.ReceiveCommandAsync(cancellationToken);

                if (command == null)
                {
                    continue;
                }

                var commandReceivers = _defaultCommandReceivers;
                try
                {
                    await Task.WhenAll(
                                commandReceivers.Select(r => CallCommandReceiver(r, command))).ConfigureAwait(false);
                }
                catch { }
            }
        }

        async Task ProcessIncomingNotifications(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var notification = await _clientChannel.ReceiveNotificationAsync(cancellationToken);

                if (notification == null)
                {
                    continue;
                }

                var notificationReceivers = _defaultNotificationReceivers;
                try
                {
                    await Task.WhenAll(
                                notificationReceivers.Select(r => CallNotificationReceiver(r, notification))).ConfigureAwait(false);
                }
                catch { }
            }
        }


        Task CallMessageReceiver(IMessageReceiver receiver, Message message)
        {
            InjectSenders(receiver);
            return receiver.ReceiveAsync(message);
        }

        Task CallCommandReceiver(ICommandReceiver receiver, Command command)
        {
            InjectSenders(receiver);
            return receiver.ReceiveAsync(command);
        }

        Task CallNotificationReceiver(INotificationReceiver receiver, Notification notification)
        {
            InjectSenders(receiver);
            return receiver.ReceiveAsync(notification);
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