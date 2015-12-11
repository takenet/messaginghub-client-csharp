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
        readonly IDictionary<MediaType, IList<IMessageReceiver>> _receivers;
        readonly IList<IMessageReceiver> _defaultReceivers = new List<IMessageReceiver> { new UnsupportedTypeMessageReceiver() };

        string _login;
        string _password;
        string _accessKey;
        string _domainName;
        IClientChannel _clientChannel;
        IClientChannelFactory _clientChannelFactory;
        ISessionFactory _sessionFactory;
        SenderWrapper sender;

        CancellationTokenSource cancellationTokenSource;
        Task backgroundExecution;
        Task messageReceiver;

        internal MessagingHubClient(IClientChannelFactory clientChannelFactory, ISessionFactory sessionFactory, string hostname = null, string domainName = null)
        {
            _receivers = new Dictionary<MediaType, IList<IMessageReceiver>>();
            _clientChannelFactory = clientChannelFactory;
            _sessionFactory = sessionFactory;
            _domainName = domainName ?? defaultDomainName;
            hostname = hostname ?? defaultDomainName;
            _endpoint = new Uri($"net.tcp://{hostname}:55321");
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

            cancellationTokenSource = new CancellationTokenSource();
            messageReceiver = Task.Run(() => ProcessIncomingMessages(cancellationTokenSource.Token));
            //Add all background tasks here. Ex: notification receiver
            backgroundExecution = Task.WhenAll(messageReceiver);

            return backgroundExecution;
        }

        /// <summary>
        /// Close connecetion and stop to receive messages from Lime server 
        /// </summary>
        /// <returns>
        public async Task StopAsync()
        {

            if (_clientChannel?.State == SessionState.Established)
            {
                    await _clientChannel.SendFinishingSessionAsync().ConfigureAwait(false);
            }
            else
            {
                    await _clientChannel.Transport.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            }

            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                await backgroundExecution;
                cancellationTokenSource.Dispose();
            }
        }

        #endregion PublicMethods

        #region InternalMethods

        void AddReceiver(MediaType mediaType, IMessageReceiver receiver)
        {
            var mediaTypeToSave = mediaType ?? MediaTypes.Any;

            IList<IMessageReceiver> mediaTypeReceivers;
            if (!_receivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<IMessageReceiver>();
                _receivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiver);
        }

        async Task ProcessIncomingMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _clientChannel.ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);

                //When cancelation 
                if(message == null)
                {
                    continue;
                }

                IList<IMessageReceiver> mimeTypeReceivers = null;
                var hasReceiver = _receivers.TryGetValue(message.Type, out mimeTypeReceivers) ||
                                  _receivers.TryGetValue(MediaTypes.Any, out mimeTypeReceivers);
                if (!hasReceiver)
                {
                    mimeTypeReceivers = _defaultReceivers;
                }

                    try
                    {
                        await Task.WhenAll(
                                mimeTypeReceivers.Select(r => CallMessageReceiver(r, message))).ConfigureAwait(false);
                    }
                    catch { }
                }
            }

        Task CallMessageReceiver(IMessageReceiver receiver, Message message)
        {
            if (receiver is MessageReceiverBase)
            {
                var receiverBase = ((MessageReceiverBase)receiver);
                receiverBase.MessageSender = sender;
                receiverBase.NotificationSender = sender;
            }

            return receiver.ReceiveAsync(message);
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
