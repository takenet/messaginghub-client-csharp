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
        private bool _started;

        private IMessagingHubConnection Connection { get; }

        private IMessagingHubListener Listener { get; }

        public IMessagingHubSender Sender { get; }

        

        public MessagingHubClient(IMessagingHubConnection connection, bool autoNotify = true)
        {
            Connection = connection;
            Sender = new MessagingHubSender(connection);
            Listener = new MessagingHubListener(connection, Sender, autoNotify);
            _semaphore = new SemaphoreSlim(1, 1);
            _started = false;
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
                _started = true;
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
                _started = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool Listening => Listener.Listening;

        public void AddMessageReceiver(IMessageReceiver messageReceiver, Func<Message, Task<bool>> messageFilter, int priority = 0)
        {
            Listener.AddMessageReceiver(messageReceiver, messageFilter, priority);
        }

        public void AddNotificationReceiver(INotificationReceiver notificationReceiver, Func<Notification, Task<bool>> notificationFilter, int priority = 0)
        {
            Listener.AddNotificationReceiver(notificationReceiver, notificationFilter, priority);
        }

        public void AddCommandReceiver(ICommandReceiver commandReceiver, Func<Command, Task<bool>> commandFilter, int priority = 0)
        {
            Listener.AddCommandReceiver(commandReceiver, commandFilter, priority);
        }

        public Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!_started) throw new InvalidOperationException("Client must be started before to proceed with this operation");
            return Sender.SendCommandAsync(command, cancellationToken);
        }

        public Task SendCommandResponseAsync(Command command, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!_started) throw new InvalidOperationException("Client must be started before to proceed with this operation");
            return Sender.SendCommandResponseAsync(command, cancellationToken);
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!_started) throw new InvalidOperationException("Client must be started before to proceed with this operation");
            await Sender.SendMessageAsync(message, cancellationToken);
        }

        public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!_started) throw new InvalidOperationException("Client must be started before to proceed with this operation");
            await Sender.SendNotificationAsync(notification, cancellationToken);
        }
    }
}