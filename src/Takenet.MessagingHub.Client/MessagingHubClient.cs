using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Security;
using Takenet.MessagingHub.Client.LimeProtocol;
using Takenet.MessagingHub.Client.Receivers;
using Lime.Protocol.Listeners;
using Lime.Protocol.Client;
using Lime.Transport.Tcp;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Network.Modules;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    ///     Default implementation for <see cref="IMessagingHubClient" /> class.
    /// </summary>
    public class MessagingHubClient : IMessagingHubClient
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _sendTimeout;
        private readonly EnvelopeListenerRegistrar _listenerRegistrar;
        private IOnDemandClientChannel _onDemandClientChannel;
        private IEstablishedClientChannelBuilder _establishedClientChannelBuilder;
        private IOnDemandClientChannelFactory _onDemandClientChannelFactory;
        private ChannelListener _channelListener;
        private static TimeSpan _channelDiscardedDelay = TimeSpan.FromMilliseconds(300);

        internal MessagingHubClient(IEstablishedClientChannelBuilder establishedClientChannelBuilder, IOnDemandClientChannelFactory onDemandClientChannelFactory, TimeSpan sendTimeout, EnvelopeListenerRegistrar listenerRegistrar)
        {
            _establishedClientChannelBuilder = establishedClientChannelBuilder;
            _onDemandClientChannelFactory = onDemandClientChannelFactory;
            _listenerRegistrar = listenerRegistrar;
            _sendTimeout = sendTimeout;
            _semaphore = new SemaphoreSlim(1);
        }

        public MessagingHubClient(Identity identity, Authentication authentication, Uri endPoint, TimeSpan sendTimeout, EnvelopeListenerRegistrar listenerRegistrar)
        {
            _semaphore = new SemaphoreSlim(1);
            _listenerRegistrar = listenerRegistrar;
            _sendTimeout = sendTimeout;

            var channelBuilder = ClientChannelBuilder.Create(() => new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer()), endPoint)
                                 .WithSendTimeout(sendTimeout)
                                 .AddMessageModule(c => new NotifyReceiptChannelModule(c))
                                 .AddCommandModule(c => new ReplyPingChannelModule(c));

            _establishedClientChannelBuilder = new EstablishedClientChannelBuilder(channelBuilder)
                                                .WithIdentity(identity)
                                                .WithAuthentication(authentication)
                                                .WithCompression(SessionCompression.None)
                                                .AddEstablishedHandler(SetPresenceAsync)
                                                .WithEncryption(SessionEncryption.TLS);
            
            _onDemandClientChannelFactory = new OnDemandClientChannelFactory();
        }

        public MessagingHubClient(Identity identity, Authentication authentication, Uri endPoint, TimeSpan sendTimeout) :
            this(identity, authentication, endPoint, sendTimeout, new EnvelopeListenerRegistrar())
        { }

        public bool Started { get; private set; }

        public virtual async Task<Command> SendCommandAsync(Command command)
        {
            if (!Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                return await _onDemandClientChannel.ProcessCommandAsync(command, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public virtual async Task SendMessageAsync(Message message)
        {
            if (!Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                await _onDemandClientChannel.SendMessageAsync(message, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public virtual async Task SendNotificationAsync(Notification notification)
        {
            if (!Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
            {
                await _onDemandClientChannel.SendNotificationAsync(notification, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public virtual Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            return _onDemandClientChannel.ReceiveMessageAsync(cancellationToken);
        }

        public virtual Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            return _onDemandClientChannel.ReceiveNotificationAsync(cancellationToken);
        }

        public virtual async Task StartAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (Started) throw new InvalidOperationException("The client is already started");
                
                _onDemandClientChannel = _onDemandClientChannelFactory.Create(_establishedClientChannelBuilder);
                _onDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscarded);

                if (_listenerRegistrar.HasRegisteredReceivers)
                {
                    StartEnvelopeListeners();
                }

                Started = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        public virtual async Task StopAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!Started) throw new InvalidOperationException("The client is not started");

                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    await _onDemandClientChannel.FinishAsync(cancellationTokenSource.Token);
                }

                if (_channelListener != null)
                {
                    _channelListener.Stop();
                    _channelListener.DisposeIfDisposable();
                }

                _onDemandClientChannel.DisposeIfDisposable();

                Started = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Task ChannelDiscarded(ChannelInformation channelInformation)
        {
            return Task.Delay(_channelDiscardedDelay);
        }

        private async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken)
        {
            await clientChannel.SetResourceAsync(
                    LimeUri.Parse(UriTemplates.PRESENCE),
                    new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity, RoundRobin = true },
                    cancellationToken)
                    .ConfigureAwait(false);
        }
        
        

        private void StartEnvelopeListeners()
        {
            var handler = new EnvelopeReceivedHandler(this, _listenerRegistrar);
            _channelListener = new ChannelListener(
                handler.HandleAsync,
                handler.HandleAsync,
                c => true.AsCompletedTask());
            _channelListener.Start(_onDemandClientChannel);
        }
        
    }
}
