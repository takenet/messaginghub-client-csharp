using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IEnvelopeReceiver
    {
        Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
        
        Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken);
    }
}