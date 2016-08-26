using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Listeners;
using Lime.Protocol.Util;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal sealed class MessagingHubListener : IMessagingHubListener
    {
        private readonly IMessagingHubConnection _connection;
        private readonly IMessagingHubSender _sender;
        private ChannelListener _channelListener;
        private CancellationTokenSource _cts;
        private MessageReceivedHandler _messageHandler;
        private NotificationReceivedHandler _notificationHandler;

        public MessagingHubListener(IMessagingHubConnection connection, IMessagingHubSender sender = null)
        {
            _connection = connection;
            _sender = sender ?? new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar(this);
        }
        
        public bool Listening { get; private set; }

        internal EnvelopeListenerRegistrar EnvelopeRegistrar { get; }

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter, int priority = 0)
        {
            EnvelopeRegistrar.AddMessageReceiver(() => messageReceiver, messageFilter, priority);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter, int priority = 0)
        {
            EnvelopeRegistrar.AddNotificationReceiver(() => notificationReceiver, notificationFilter, priority);
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            StartEnvelopeListeners();
            Listening = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            StopEnvelopeListeners();
            Listening = false;
            return Task.CompletedTask;
        }

        private void StartEnvelopeListeners()
        {
            _cts = new CancellationTokenSource();
            _messageHandler = new MessageReceivedHandler(_sender, EnvelopeRegistrar, _cts);
            _notificationHandler = new NotificationReceivedHandler(_sender, EnvelopeRegistrar, _cts);
            
            _channelListener = new ChannelListener(
                m => _messageHandler.HandleAsync(m, _cts.Token),
                n => _notificationHandler.HandleAsync(n, _cts.Token),
                c => TaskUtil.TrueCompletedTask);
            _channelListener.Start(_connection.OnDemandClientChannel);
        }

        private void StopEnvelopeListeners()
        {
            _channelListener?.Stop();
            _channelListener?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
