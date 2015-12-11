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
    public class MessagingHubClient
    {
        static readonly MediaType defaultReceiverMediaType = new MediaType("null", "null");

        public IMessageSender MessageSender { get; private set; }

        readonly Uri _endpoint;
        readonly IDictionary<MediaType, IList<IMessageReceiver>> _receivers;
        readonly TaskCompletionSource<bool> _running;

        string _login;
        string _password;
        string _accessKey;
        IClientChannel _clientChannel;
        IClientChannelFactory _clientChannelFactory;
        ISessionFactory _sessionFactory;
            
        internal MessagingHubClient(IClientChannelFactory clientChannelFactory, ISessionFactory sessionFactory, string hostname)
        {
            _receivers = new Dictionary<MediaType, IList<IMessageReceiver>>();
            _running = new TaskCompletionSource<bool>();
            _clientChannelFactory = clientChannelFactory;
            _sessionFactory = sessionFactory;
            _endpoint = new Uri($"net.tcp://{hostname}:55321");
        }

        public MessagingHubClient(string hostname) : 
            this(new ClientChannelFactory(), new SessionFactory(), hostname)
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

            _clientChannel = await _clientChannelFactory.CreateClientChannelAsync(_endpoint);

            var session = await _sessionFactory.CreateSessionAsync(_clientChannel, _login, authentication);

            if (session.State != SessionState.Established)
            {
                throw new LimeException(session.Reason.Code, session.Reason.Description);
            }

            MessageSender = new MessageSenderWrapper(_clientChannel);
            await _clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { RoutingRule = RoutingRule.Identity },
                CancellationToken.None);

            StartReceiving();

            return _running.Task;
        }

        /// <summary>
        /// Close connecetion and stop to receive messages from Lime server 
        /// </summary>
        /// <returns>
        public async Task StopAsync()
        {
            try
            {
                if (_clientChannel?.State == SessionState.Established)
                {
                    await _clientChannel.SendFinishingSessionAsync();
                }
                else
                {
                    await _clientChannel.Transport.CloseAsync(CancellationToken.None);
                }

                _running.SetResult(true);
            }
            catch (Exception e)
            {
                _running.SetException(e);
            }
        }

        #endregion PublicMethods

        #region InternalMethods

        void AddReceiver(MediaType mediaType, IMessageReceiver receiver)
        {
            var mediaTypeToSave = mediaType ?? defaultReceiverMediaType;

            IList<IMessageReceiver> mediaTypeReceivers;
            if (!_receivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<IMessageReceiver>();
                _receivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiver);
        }

        void StartReceiving()
        {
            Task.Run(ProcessIncomingMessages);
        }

        async Task ProcessIncomingMessages()
        {
            while (_running.Task.Status == TaskStatus.Running)
            {
                var message = await _clientChannel.ReceiveMessageAsync(CancellationToken.None);
                IList<IMessageReceiver> mimeTypeReceivers = null;
                if (_receivers.TryGetValue(message.Type, out mimeTypeReceivers) ||
                    _receivers.TryGetValue(MediaTypes.Any, out mimeTypeReceivers) ||
                    _receivers.TryGetValue(defaultReceiverMediaType, out mimeTypeReceivers))
                {
                    await Task.WhenAll(
                        mimeTypeReceivers.Select(r => CallMessageReceiver(r, message)));
                }
            }
        }

        Task CallMessageReceiver(IMessageReceiver receiver, Message message)
        {
            if (receiver is MessageReceiverBase)
            {
                ((MessageReceiverBase)receiver).Sender = MessageSender;
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