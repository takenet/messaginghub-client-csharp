using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    /// <summary>
    /// Defines a service for handling exception that occurs in the text processing.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the exception is handled; false otherwise.</returns>
        Task<bool> HandleExceptionAsync(Exception exception, Message message, IRequestContext context, CancellationToken cancellationToken);
    }
}