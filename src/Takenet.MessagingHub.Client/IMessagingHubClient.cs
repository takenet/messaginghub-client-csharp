using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IMessagingHubClient
    {
        IMessageSender MessageSender { get; }
        
        MessagingHubClient AddMessageReceiver(IMessageReceiver receiver, MediaType forMimeType = null);
        
        Task<Task> StartAsync();

        Task StopAsync();
    }
}
