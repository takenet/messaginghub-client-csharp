using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    public class TextResponseMessageReceiver : MessageReceiverBase
    {
        private static string _text;

        public TextResponseMessageReceiver(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _text = text;
        }

        public override Task ReceiveAsync(Message message)
        {
            return EnvelopeSender.SendMessageAsync(_text, message.GetSender());
        }
    }
}
