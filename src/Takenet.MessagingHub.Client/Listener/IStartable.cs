using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Represents a type that can be started
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// Starts the operation
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}