using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Connection;

namespace Takenet.MessagingHub.Client.Sender
{
    public sealed class MessagingHubSender
    {
        public MessagingHubConnection Connection { get; set; }

        public MessagingHubSender(MessagingHubConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Send a command through the Messaging Hub
        /// </summary>
        /// <param name="command">Command to be sent</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation. When completed, it will contain the command response</returns>
        public async Task<Command> SendCommandAsync(Command command, CancellationToken token)
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

        /// <summary>
        /// Send a message through the Messaging Hub
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        public async Task SendMessageAsync(Message message, CancellationToken token)
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

        /// <summary>
        /// Send a notification through the Messaging Hub
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <param name="token">A cancellation token to allow the task to be canceled</param>
        /// <returns>A task representing the sending operation</returns>
        public async Task SendNotificationAsync(Notification notification, CancellationToken token)
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("A connection must be estabilished before to proceed with this operation!");

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
