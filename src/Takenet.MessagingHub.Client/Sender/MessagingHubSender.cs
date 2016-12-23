using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client.Sender
{
    internal sealed class MessagingHubSender : IMessagingHubSender
    {
        private IMessagingHubConnection Connection { get; }

        public MessagingHubSender(IMessagingHubConnection connection)
        {
            Connection = connection;
        }

        public Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            return Connection.OnDemandClientChannel.ProcessCommandAsync(command, cancellationToken);
        }

        public Task SendCommandResponseAsync(Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            return Connection.OnDemandClientChannel.SendCommandAsync(command, cancellationToken);
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            return Connection.OnDemandClientChannel.SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        /// Dispatch a notification, if its id is not null or empty.
        /// </summary>
        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(notification.Id)) return Task.CompletedTask;

            if (!Connection.IsConnected)
                throw new InvalidOperationException("A connection must be established before to proceed with this operation!");

            return Connection.OnDemandClientChannel.SendNotificationAsync(notification, cancellationToken);
        }
    }
}
