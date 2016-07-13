using System.Collections.Generic;

namespace Takenet.MessagingHub.Client.Textc
{
    public class TextcMessageReceiverCommandSettings
    {
        public string[] Syntaxes { get; set; }

        public string ReturnText { get; set; }

        public Dictionary<string, object> ReturnJson { get; set; }
            
        public string ReturnJsonMediaType { get; set; }

        public string ProcessorType { get; set; }

        public string Method { get; set; }
    }
}