using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    /// <summary>
    /// Defines a service for providing session context.
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        /// Gets an <see cref="IRequestContext"/> instance for the given sender and destination.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        Task<IRequestContext> GetContextAsync(Node sender, Node destination);

        /// <summary>
        /// Saves an <see cref="IRequestContext"/> instance for the given sender and destination.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task SaveContextAsync(Node sender, Node destination, IRequestContext context);
    }
}