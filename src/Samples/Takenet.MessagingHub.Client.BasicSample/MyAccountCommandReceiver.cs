using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.BasicSample
{
    class MyAccountCommandReceiver : CommandReceiverBase
    {
        public override Task ReceiveAsync(Command command)
        {
            if(command.Uri.ToString() == "/account" )
            {
                Console.WriteLine("Received account: {0}", command.From);
            }

            return Task.FromResult(0);
        }
    }
}
