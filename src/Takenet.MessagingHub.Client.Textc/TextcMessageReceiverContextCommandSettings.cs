using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverContextCommandSettings
    {
        public string Type { get; set; }

        public Dictionary<string, object> Parameters { get; set; }
    }
}
