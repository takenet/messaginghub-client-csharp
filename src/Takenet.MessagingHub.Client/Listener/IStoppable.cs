using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    public interface IStoppable
    {
        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
