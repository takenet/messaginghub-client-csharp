using System.Threading;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class CommandReceivedHandler : EnvelopeReceivedHandler<Command>
    {
        public CommandReceivedHandler(IMessagingHubSender sender, EnvelopeReceiverManager envelopeManager, CancellationTokenSource cts)
            : base(sender, envelopeManager, cts)
        { }
    }
}