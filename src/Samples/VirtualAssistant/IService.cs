using System.Threading;
using System.Threading.Tasks;

namespace Takenet.VirtualAssistant
{
    /// <summary>
    /// Defines a generic service.
    /// </summary>
    public interface IService
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Stop();
        Task Execution { get; }
    }
}