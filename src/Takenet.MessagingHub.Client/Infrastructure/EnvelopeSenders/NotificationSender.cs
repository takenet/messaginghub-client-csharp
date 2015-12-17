using Lime.Protocol;
using System;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Senders
{
    internal class NotificationSender : EnvelopeSenderBase<Notification>, INotificationSender
    {
        public NotificationSender(IEnvelopeProcessor<Notification> processor, TimeSpan timeout)
            : base(processor, timeout)
        {
        }
    }
}
