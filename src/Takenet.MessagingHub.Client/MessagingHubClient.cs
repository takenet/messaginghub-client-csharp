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
        private IMessagingHubConnection Connection { get; }

        private IMessagingHubListener Listener { get; }

        public IMessagingHubSender Sender { get; }

        public MessagingHubClient(IMessagingHubConnection connection)
        {
            Connection = connection;
            Sender = new MessagingHubSender(connection);
            Listener = new MessagingHubListener(connection, Sender);
        }

        public async Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await Connection.ConnectAsync(cancellationToken);
            await Listener.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await Listener.StopAsync(cancellationToken);
            await Connection.DisconnectAsync(cancellationToken);
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

        public Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = new CancellationToken())
        {
            return Sender.SendCommandAsync(command, cancellationToken);
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