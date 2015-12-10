using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public abstract class MessageReceiverBase : IMessageReceiver
    {
        public IMessageSender Sender { get; internal set; }

        public abstract Task ReceiveAsync(Message message);
    }
}
