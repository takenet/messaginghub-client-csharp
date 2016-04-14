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

        public async Task<Command> SendCommandAsync(Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        cancellationToken))
                {
                    return
                        await
                            Connection.OnDemandClientChannel.ProcessCommandAsync(command, linkedTokenSource.Token)
                                .ConfigureAwait(false);
                }
            }
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Client must be started before to proceed with this operation");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        cancellationToken))
                {
                    await
                        Connection.OnDemandClientChannel.SendMessageAsync(message, linkedTokenSource.Token)
                            .ConfigureAwait(false);
                }
            }
        }

        public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("A connection must be established before to proceed with this operation!");

            using (var timeoutTokenSource = new CancellationTokenSource(Connection.SendTimeout))
            {
                using (
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token,
                        cancellationToken))
                {
                    await
                        Connection.OnDemandClientChannel.SendNotificationAsync(notification,
                            linkedTokenSource.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
