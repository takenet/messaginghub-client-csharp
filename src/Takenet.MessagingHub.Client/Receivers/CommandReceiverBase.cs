using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public abstract class CommandReceiverBase : ReceiverBase, ICommandReceiver
    {
        public abstract Task ReceiveAsync(Command command);
    }
}
