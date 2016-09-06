using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Scheduler
{
    public class SchedulerExtension : ExtensionBase, ISchedulerExtension
    {
        private static readonly Node SchedulerAddress = Node.Parse($"postmaster@scheduler.{Constants.DEFAULT_DOMAIN}");

        public SchedulerExtension(IMessagingHubSender sender) 
            : base(sender)
        {
        }

        public Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return ProcessCommandAsync(
                CreateSetCommandRequest(
                    new Schedule()
                    {
                        Message = message,
                        When = when
                    },
                    "/schedules",
                    SchedulerAddress),
                cancellationToken);
        }
    }
}