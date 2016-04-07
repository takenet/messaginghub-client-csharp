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

        public virtual async Task<Command> SendCommandAsync(Command command, CancellationToken token)
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        token))
                {
                    return
                        await
                            Connection.OnDemandClientChannel.ProcessCommandAsync(command, linkedTokenSource.Token)
                                .ConfigureAwait(false);
                }
            }
        }

        public virtual async Task SendMessageAsync(Message message, CancellationToken token)
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        token))
                {
                    await
                        Connection.OnDemandClientChannel.SendMessageAsync(message, linkedTokenSource.Token)
                            .ConfigureAwait(false);
                }
            }
        }

        public virtual async Task SendNotificationAsync(Notification notification, CancellationToken token)
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation!");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        token))
                {
                    await
                        Connection.OnDemandClientChannel.SendNotificationAsync(notification,
                            linkedTokenSource.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
