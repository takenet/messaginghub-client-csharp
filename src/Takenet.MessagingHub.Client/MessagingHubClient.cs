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
using Lime.Protocol.Util;

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
        private readonly IEstablishedClientChannelBuilder _establishedClientChannelBuilder;
        private readonly IOnDemandClientChannelFactory _onDemandClientChannelFactory;

        private IOnDemandClientChannel _onDemandClientChannel;
        private ChannelListener _channelListener;

        private static readonly TimeSpan ChannelDiscardedDelay = TimeSpan.FromMilliseconds(300);

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
                if (Started)
                    throw new InvalidOperationException("The client is already started");
                
                _onDemandClientChannel = _onDemandClientChannelFactory.Create(_establishedClientChannelBuilder);
                _onDemandClientChannel.ChannelDiscardedHandlers.Add(ChannelDiscarded);

                if (_listenerRegistrar.HasRegisteredReceivers)
                    StartEnvelopeListeners();

                for (var i = 0; i < 3; i++)
                {
                    if (await EnsureConnectionIsOkayAsync()) 
                    {
                        Started = true;
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }

                throw new TimeoutException("Could not connect to server!");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<bool> EnsureConnectionIsOkayAsync()
        {
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource(_sendTimeout))
                {
                    var command = new Command
                    {
                        Method = CommandMethod.Get,
                        Uri = new LimeUri(UriTemplates.PING)
                    };

                    var result =
                        await
                            _onDemandClientChannel.ProcessCommandAsync(command, cancellationTokenSource.Token)
                                .ConfigureAwait(false);
                    return result.Status == CommandStatus.Success;
                }
            }
            catch (Exception)
            {
                return false;
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
                    await _onDemandClientChannel.FinishAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }

                if (_channelListener != null)
                {
                    _channelListener.Stop();

                    // TODO: Signal to the listener to stop consuming the envelopes.
                    //await Task.WhenAll(
                    //    _channelListener.CommandListenerTask, 
                    //    _channelListener.MessageListenerTask,
                    //    _channelListener.NotificationListenerTask)
                    //    .ConfigureAwait(false);

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
            return Task.Delay(ChannelDiscardedDelay);
        }

        private async Task SetPresenceAsync(IClientChannel clientChannel, CancellationToken cancellationToken)
        {
            if (!IsGuest(clientChannel.LocalNode.Name))
                await clientChannel.SetResourceAsync(
                        LimeUri.Parse(UriTemplates.PRESENCE),
                        new Presence { Status = PresenceStatus.Available, RoutingRule = RoutingRule.Identity, RoundRobin = true },
                        cancellationToken)
                        .ConfigureAwait(false);
        }

        public static bool IsGuest(string name)
        {
            Guid g;
            return Guid.TryParse(name, out g);
        }

        private void StartEnvelopeListeners()
        {
            var handler = new EnvelopeReceivedHandler(this, _listenerRegistrar);
            _channelListener = new ChannelListener(
                handler.HandleAsync,
                handler.HandleAsync,
                c => TaskUtil.TrueCompletedTask);
            _channelListener.Start(_onDemandClientChannel);
        }        
    }
}
