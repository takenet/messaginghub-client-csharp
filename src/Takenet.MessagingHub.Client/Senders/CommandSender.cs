using Lime.Protocol;
using System;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Senders
{
    internal class CommandSender : EnvelopeSenderBase<Command>, ICommandSender
    {
        public CommandSender(IEnvelopeProcessor<Command> processor, TimeSpan timeout)
            : base(processor, timeout)
        {
        }
    }
}
