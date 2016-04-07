using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated
{
    [Obsolete]
    public interface IEnvelopeReceiver
    {
        Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
        
        Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken);
    }
}