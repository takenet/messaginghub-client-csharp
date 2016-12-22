using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Notification receiver that simply ignores the received notification
    /// </summary>
    public class BlackholeReceiver : IEnvelopeReceiver<Envelope>
    {
        public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}