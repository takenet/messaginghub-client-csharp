using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Represents a type that can be stopped
    /// </summary>
    public interface IStoppable
    {
        /// <summary>
        /// Stops the operation
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to allow the operation to be canceled</param>
        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
