using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public class TextResponseMessageReceiver : MessageReceiverBase
    {
        private static string _text;

        public TextResponseMessageReceiver(IDictionary<string, object> settings)
        {
            if (!settings.ContainsKey("text")) throw new ArgumentException("The key 'text' was not found");
            _text = (string)settings["text"];
        }

        public override Task ReceiveAsync(Message message)
        {
            return EnvelopeSender.SendMessageAsync(_text, message.GetSender());
        }
    }
}
