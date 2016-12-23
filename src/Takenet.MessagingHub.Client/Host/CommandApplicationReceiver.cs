using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Host
{
    public class CommandApplicationReceiver : ApplicationReceiver
    {
        public CommandMethod? Method { get; set; }

        public string Uri { get; set; }

        public string ResourceUri { get; set; }
       
        public string MediaType { get; set; }
    }
}
