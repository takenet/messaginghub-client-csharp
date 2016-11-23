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
        private readonly bool _autoNotify;
        private readonly IMessagingHubConnection _connection;
        private readonly IMessagingHubSender _sender;
        private ChannelListener _channelListener;
        private CancellationTokenSource _cts;

        public MessagingHubListener(IMessagingHubConnection connection, IMessagingHubSender sender = null, bool autoNotify = true)
        {
            _connection = connection;
            _sender = sender ?? new MessagingHubSender(connection);
            EnvelopeRegistrar = new EnvelopeListenerRegistrar(this);
            _autoNotify = autoNotify;
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
            var messageHandler = new MessageReceivedHandler(_sender, _autoNotify, EnvelopeRegistrar, _cts);
            var notificationHandler = new NotificationReceivedHandler(_sender, EnvelopeRegistrar, _cts);
            
            _channelListener = new ChannelListener(
                m => messageHandler.HandleAsync(m, _cts.Token),
                n => notificationHandler.HandleAsync(n, _cts.Token),
                c => TaskUtil.TrueCompletedTask);
            _channelListener.Start(_connection.OnDemandClientChannel);
        }

        private void StopEnvelopeListeners()
        {
            try
            {
                _channelListener?.Stop();
                _cts?.Cancel();
                _channelListener?.Dispose();
                _cts?.Dispose();
            }
            catch (ObjectDisposedException) { }
        }
    }
}
