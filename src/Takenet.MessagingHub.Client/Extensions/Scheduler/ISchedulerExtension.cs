﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources;

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

        /// <summary>
        /// Get scheduled message details, including status.
        /// </summary>
        /// <param name="messageId">Id of the scheduled message</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Schedule> GetScheduledMessageAsync(string messageId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Cancels a scheduled message.
        /// </summary>
        /// <param name="messageId">Id of the scheduled message</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task CancelScheduledMessageAsync(string messageId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
