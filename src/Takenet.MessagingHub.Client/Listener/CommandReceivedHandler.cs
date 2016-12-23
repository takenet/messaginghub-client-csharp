using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Listener
{
    internal class CommandReceivedHandler : EnvelopeReceivedHandler<Command>
    {
        public CommandReceivedHandler(IMessagingHubSender sender, EnvelopeReceiverManager envelopeManager, CancellationTokenSource cts)
            : base(sender, envelopeManager, cts)
        { }

        protected override async Task CallReceiversAsync(Command command, CancellationToken cancellationToken)
        {
            try
            {
                await base.CallReceiversAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                await Sender.SendCommandResponseAsync(new Command
                {
                    Id = command.Id,
                    To = command.From,
                    Method = command.Method,
                    Status = CommandStatus.Failure,
                    Reason = ex.ToReason(),
                }, cancellationToken);
            }
        }
    }
}