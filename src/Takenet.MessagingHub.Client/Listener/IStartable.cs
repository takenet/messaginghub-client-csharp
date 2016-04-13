using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    public interface IStartable
    {
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}