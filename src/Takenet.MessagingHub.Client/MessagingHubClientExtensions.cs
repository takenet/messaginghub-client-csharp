using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        public static Task SendMessageAsync(this MessagingHubClient client, string content, string to) => client.MessageSender.SendMessageAsync(content, to);
    }
}
