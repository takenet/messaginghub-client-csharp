using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client
{
    public class MessagingHubClient : IMessagingHubClient
    {
        private readonly SemaphoreSlim _semaphore;

        private IMessagingHubConnection Connection { get; }

        private IMessagingHubListener Listener { get; }

        public IMessagingHubSender Sender { get; }

        public MessagingHubClient(IMessagingHubConnection connection, bool autoNotify = true)
        {
            Connection = connection;
            Sender = new MessagingHubSender(connection);
            Listener = new MessagingHubListener(connection, Sender, autoNotify);
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!Listening)
                {
                    await Connection.ConnectAsync(cancellationToken);
                    await Listener.StartAsync(cancellationToken);                    
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (Listening)
                {
                    await Listener.StopAsync(cancellationToken);
                    await Connection.DisconnectAsync(cancellationToken);                    
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool Listening => Listener.Listening;

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Predicate<Message> messageFilter, int priority = 0)
        {
            Listener.AddMessageReceiver(messageReceiver, messageFilter, priority);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Predicate<Notification> notificationFilter, int priority = 0)
        {
            Listener.AddNotificationReceiver(notificationReceiver, notificationFilter, priority);
        }

        public void AddCommandReceiver(ICommandReceiver commandReceiver, Predicate<Command> commandFilter, int priority = 0)
        {
            Listener.AddCommandReceiver(commandReceiver, commandFilter, priority);
        }

        public Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = new CancellationToken())
        {
            return Sender.SendCommandAsync(command, cancellationToken);
        }

        public Task SendCommandResponseAsync(Command command, CancellationToken cancellationToken = new CancellationToken())
        {
            return Sender.SendCommandResponseAsync(command, cancellationToken);
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            await Sender.SendMessageAsync(message, cancellationToken);
        }

        public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = new CancellationToken())
        {
            await Sender.SendNotificationAsync(notification, cancellationToken);
        }
    }
}