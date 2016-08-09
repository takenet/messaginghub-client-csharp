using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Extensions.Scheduler
{
    /// <summary>
    /// Defines a service that allows the scheduling of messages for sending.
    /// </summary>
    public interface ISchedulerExtension
    {
        /// <summary>
        /// Schedules a message to the specified date.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="when">The when.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ScheduleMessageAsync(Message message, DateTimeOffset when, CancellationToken cancellationToken = default(CancellationToken));
    }
}
