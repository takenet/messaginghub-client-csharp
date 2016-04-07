using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client.Sender
{
    public class MessagingHubSender : IMessagingHubSender
    {
        public MessagingHubConnection Connection { get; set; }

        internal MessagingHubSender(MessagingHubConnection connection)
        {
            Connection = connection;
        }

        public virtual async Task<Command> SendCommandAsync(Command command)
        {
            if (!Connection.Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var cancellationTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                return await Connection.OnDemandClientChannel.ProcessCommandAsync(command, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public virtual async Task SendMessageAsync(Message message)
        {
            if (!Connection.Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var cancellationTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                await Connection.OnDemandClientChannel.SendMessageAsync(message, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public virtual async Task SendNotificationAsync(Notification notification)
        {
            if (!Connection.Started)
                throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            using (var cancellationTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                await Connection.OnDemandClientChannel.SendNotificationAsync(notification, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
