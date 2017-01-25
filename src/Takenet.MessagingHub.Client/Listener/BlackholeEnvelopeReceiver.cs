using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Notification receiver that simply ignores the received envelope.
    /// </summary>
    public class BlackholeEnvelopeReceiver : IEnvelopeReceiver<Envelope>
    {
        public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
    }
}