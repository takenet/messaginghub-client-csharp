using Lime.Protocol;
using System.Globalization;

namespace Takenet.MessagingHub.Client.Textc
{
    /// <summary>
    /// Provides culture information to be used by the session context.
    /// </summary>
    public interface ICultureProvider
    {
        /// <summary>
        /// Gets the culture information for the specified node.
        /// </summary>
        /// <param name = "node"></param>
        /// <returns></returns>
        CultureInfo GetCulture(Node node);
    }
}