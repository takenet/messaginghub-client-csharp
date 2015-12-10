using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Client;
using Lime.Protocol.Network;
using Lime.Protocol.Security;
using Lime.Transport.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClient
    {
        static readonly MediaType defaultReceiverMediaType = new MediaType("null", "null");

        public IMessageSender MessageSender { get; private set; }

        readonly Uri endpoint;
        readonly IDictionary<MediaType, IList<IMessageReceiver>> receivers;
        readonly TaskCompletionSource<bool> running;

        string login;
        string password;
        string accessKey;
        IClientChannel clientChannel;

        MessagingHubClient()
        {
            receivers = new Dictionary<MediaType, IList<IMessageReceiver>>();
            running = new TaskCompletionSource<bool>();
        }

        public MessagingHubClient(string hostname) : this()
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="forMimeType">When not informed, only receives messages which no 'typed' receiver is registered</param>
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

            clientChannel = await CreateAndOpenAsync();

            var session = await clientChannel.EstablishSessionAsync(
                _ => SessionCompression.None,
                _ => SessionEncryption.TLS,
                Identity.Parse(this.login),
                (_, __) => authentication,
                Environment.MachineName,
                CancellationToken.None);

            if (session.State != SessionState.Established) throw new Exception($"Could not connect: {session.Reason.Description} (code: {session.Reason.Code})");

            MessageSender = new MessageSenderWrapper(clientChannel);
            await clientChannel.SetResourceAsync(
                LimeUri.Parse(UriTemplates.PRESENCE),
                new Presence { RoutingRule = RoutingRule.Identity },
                CancellationToken.None);

            StartReceiving();

            return running.Task;
        }


        public async Task StopAsync()
        {
            try
            {
                if (clientChannel?.State == SessionState.Established)
                {
                    await clientChannel.SendFinishingSessionAsync();
                }
                else
                {
                    await clientChannel.Transport.CloseAsync(CancellationToken.None);
                }

                running.SetResult(true);
            }
            catch (Exception e)
            {
                running.SetException(e);
            }
        }

        void StartReceiving()
        {
            Task.Run(ProcessIncomingMessages);
        }

        async Task ProcessIncomingMessages()
        {
            while (running.Task.Status == TaskStatus.Running)
            {
                var message = await clientChannel.ReceiveMessageAsync(CancellationToken.None);
                IList<IMessageReceiver> mimeTypeReceivers = null;
                if (receivers.TryGetValue(message.Type, out mimeTypeReceivers) ||
                    receivers.TryGetValue(MediaTypes.Any, out mimeTypeReceivers) ||
                    receivers.TryGetValue(defaultReceiverMediaType, out mimeTypeReceivers))
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

        async Task<ClientChannel> CreateAndOpenAsync()
        {
            var transport = new TcpTransport(traceWriter: new TraceWriter());
            var sendTimeout = TimeSpan.FromSeconds(3);

            using (var cancellationTokenSource = new CancellationTokenSource(sendTimeout))
            {
                await transport.OpenAsync(this.endpoint, cancellationTokenSource.Token);
            }

            var clientChannel = new ClientChannel(transport, sendTimeout);

            return clientChannel;
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

            if (result == null) throw new InvalidOperationException($"A password (method {nameof(UsingAccount)}) or accessKey (method {nameof(UsingAccessKey)}) should be informed");

            return result;
        }

        void AddReceiver(MediaType mediaType, IMessageReceiver receiver)
        {
            var mediaTypeToSave = mediaType ?? defaultReceiverMediaType;

            IList<IMessageReceiver> mediaTypeReceivers;
            if (!receivers.TryGetValue(mediaTypeToSave, out mediaTypeReceivers))
            {
                mediaTypeReceivers = new List<IMessageReceiver>();
                receivers.Add(mediaTypeToSave, mediaTypeReceivers);
            }

            mediaTypeReceivers.Add(receiver);
        }
    }
}
