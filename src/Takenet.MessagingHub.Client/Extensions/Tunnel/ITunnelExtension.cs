using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Tunnel
{
    /// <summary>
    /// Allows forwarding envelopes to an identity.
    /// </summary>
    public interface ITunnelExtension
    {
        Task<Node> ForwardMessageAsync(Message message, Identity destination, CancellationToken cancellationToken);

        Task<Node> ForwardNotificationAsync(Notification notification, Identity destination, CancellationToken cancellationToken);
    }
}
