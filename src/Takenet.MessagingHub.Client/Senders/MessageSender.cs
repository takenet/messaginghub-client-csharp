using Lime.Protocol;
using System;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Senders
{
    internal class MessageSender : EnvelopeSenderBase<Message>, IMessageSender
    {
        public MessageSender(IEnvelopeProcessor<Message> processor, TimeSpan timeout)
            : base(processor, timeout)
        {
        }
    }
}
