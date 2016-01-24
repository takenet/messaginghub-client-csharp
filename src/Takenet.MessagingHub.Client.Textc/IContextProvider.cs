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
        /// Gets a <see cref="IRequestContext"/> instance for the given sender and destination.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        IRequestContext GetContext(Node sender, Node destination);
    }
}