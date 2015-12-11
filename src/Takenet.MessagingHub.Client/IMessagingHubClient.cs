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
        
        MessagingHubClient AddMessageReceiver(IMessageReceiver receiver, MediaType forMimeType = null);
        
        Task StartAsync();

        Task StopAsync();
    }
}
