using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    public abstract class MessageReceiverBase : ReceiverBase, IMessageReceiver
    {
        public abstract Task ReceiveAsync(Message message);
    }
}
