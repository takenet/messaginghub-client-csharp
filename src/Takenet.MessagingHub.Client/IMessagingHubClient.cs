using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    public interface IMessagingHubClient
    {
        IMessageSender MessageSender { get; }

        ICommandSender CommandSender { get; }

        INotificationSender NotificationSender { get; }

        MessagingHubClient AddMessageReceiver(IMessageReceiver receiver, MediaType forMimeType = null);

        MessagingHubClient AddNotificationReceiver(INotificationReceiver receiver, Event? forEventType = null);

        Task StartAsync();

        Task StopAsync();
    }
}
