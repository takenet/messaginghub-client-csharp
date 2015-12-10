using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IMessageSender
    {
        Task SendMessageAsync(Message message);
        Task SendMessageAsync(string content, string to);
        Task SendMessageAsync(string content, Node to);
    }
}
