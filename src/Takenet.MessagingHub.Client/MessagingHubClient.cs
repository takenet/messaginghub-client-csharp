using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using Lime.Transport.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClient : IMessagingHubClient
    {
        static readonly MediaType defaultReceiverMediaType = new MediaType("null", "null");
        static readonly string defaultDomainName = "msging.net";

        public IMessageSender MessageSender { get; private set; }

        readonly Uri endpoint;
        readonly IDictionary<MediaType, IList<IMessageReceiver>> receivers;

        string domainName;
        string login;
        string password;
        string accessKey;
        IClientChannel clientChannel;

        CancellationTokenSource cancellationTokenSource;
        Task backgroundExecution;
        Task messageReceiver;

        MessagingHubClient()
        {
            receivers = new Dictionary<MediaType, IList<IMessageReceiver>>();
        }

        public MessagingHubClient(string hostname = null, string domainName = null) : this()
        {
            this.domainName = domainName ?? defaultDomainName;
            hostname = hostname ?? defaultDomainName;
            this.endpoint = new Uri($"net.tcp://{hostname}:55321");
        }

        public MessagingHubClient UsingAccount(string login, string password)
        {
            this.login = login;
            this.password = password;
            return this;
        }

        public MessagingHubClient UsingAccessKey(string login, string key)
        {
            this.login = login;
            this.accessKey = key;
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

            clientChannel = await CreateAndOpenAsync().ConfigureAwait(false);

            var session = await EstablishSession(authentication).ConfigureAwait(false);

            if (session.State != SessionState.Established)
            {
                throw new Exception($"Could not connect: {session.Reason.Description} (code: {session.Reason.Code})");
            }

            MessageSender = new MessageSenderWrapper(clientChannel);
            await clientChannel.SetResourceAsync(
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

            if (clientChannel?.State == SessionState.Established)
            {
                await clientChannel.SendFinishingSessionAsync().ConfigureAwait(false);
            }
            else
            {
                await clientChannel.Transport.CloseAsync(CancellationToken.None).ConfigureAwait(false);
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
            if (!receivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<IMessageReceiver>();
                receivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiver);
        }

        async Task ProcessIncomingMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await clientChannel.ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);

                //When cancelation 
                if(message == null)
                {
                    continue;
                }

                IList<IMessageReceiver> mimeTypeReceivers = null;
                if (receivers.TryGetValue(message.Type, out mimeTypeReceivers) ||
                    receivers.TryGetValue(MediaTypes.Any, out mimeTypeReceivers) ||
                    receivers.TryGetValue(defaultReceiverMediaType, out mimeTypeReceivers))
                {
                    try
                    {
                        await Task.WhenAll(
                                mimeTypeReceivers.Select(r => CallMessageReceiver(r, message))).ConfigureAwait(false);
                    }
                    catch { }
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

        internal virtual async Task<IClientChannel> CreateAndOpenAsync()
        {
            var transport = new TcpTransport(traceWriter: new TraceWriter());
            var sendTimeout = TimeSpan.FromSeconds(3);

            using (var cancellationTokenSource = new CancellationTokenSource(sendTimeout))
            {
                await transport.OpenAsync(this.endpoint, cancellationTokenSource.Token).ConfigureAwait(false);
            }

            var clientChannel = new ClientChannel(transport, sendTimeout);

            return clientChannel;
        }

        internal virtual Task<Session> EstablishSession(Authentication authentication)
        {
            return clientChannel.EstablishSessionAsync(
                        _ => SessionCompression.None,
                        _ => SessionEncryption.TLS,
                        Identity.Parse($"{this.login}@{this.domainName}"),
                        (_, __) => authentication,
                        Environment.MachineName,
                        CancellationToken.None);
        }

        Authentication GetAuthenticationScheme()
        {
            Authentication result = null;

            if (this.password != null)
            {
                var plainAuthentication = new PlainAuthentication();
                plainAuthentication.SetToBase64Password(this.password);
                result = plainAuthentication;
            }

            if (this.accessKey != null)
            {
                var keyAuthentication = new KeyAuthentication { Key = accessKey };
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
