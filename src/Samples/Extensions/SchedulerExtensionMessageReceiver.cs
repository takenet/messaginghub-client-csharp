using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using System.Diagnostics;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Lime.Messaging.Contents;

namespace Extensions
{
    public class SchedulerExtensionMessageReceiver : IMessageReceiver
    {
        private readonly ISchedulerExtension _schedulerExtension;
        private readonly IMessagingHubSender _sender;

        public SchedulerExtensionMessageReceiver(IMessagingHubSender sender, ISchedulerExtension schedulerExtension)
        {
            _schedulerExtension = schedulerExtension;
            _sender = sender;
        }

        //Schedule a message to next 10 minutes
        public async Task ReceiveAsync(Message receivedMessage, CancellationToken cancellationToken)
        {
            var schedullingDate = DateTimeOffset.Now.AddMinutes(10);
            var messageContent = "tomar remédio";

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = receivedMessage.From,
                Content = new PlainText { Text = messageContent }
            };

            await _schedulerExtension.ScheduleMessageAsync(message, schedullingDate);
        }
    }
}
