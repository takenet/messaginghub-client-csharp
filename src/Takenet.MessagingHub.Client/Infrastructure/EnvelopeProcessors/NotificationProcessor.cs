using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Send and receive commands
    /// </summary>
    internal class NotificationProcessor : EnvelopeProcessor<Notification>
    {
        private readonly IClientChannel _clientChannel;

        public NotificationProcessor(IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
        }

        protected override Task<Notification> ReceiveAsync(CancellationToken cancellationToken)
        {
            return _clientChannel.ReceiveNotificationAsync(cancellationToken);
        }

        protected override Task SendAsync(Notification envelope, CancellationToken cancellationToken)
        {
            return _clientChannel.SendNotificationAsync(envelope);
        }

        public override Task<Notification> SendAsync(Notification notification, TimeSpan timeout)
        {
            return base.SendAsync(notification, timeout);
        }
    }
}
