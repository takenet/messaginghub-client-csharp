using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IMessageReceiver
    {
        Task ReceiveAsync(Message message);
    }
}
