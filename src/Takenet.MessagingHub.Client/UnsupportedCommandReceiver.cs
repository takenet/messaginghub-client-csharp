using System;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client
{
    internal class UnsupportedCommandReceiver : ReceiverBase, ICommandReceiver
    {
        public Task ReceiveAsync(Command command)
        {
            var responseCommand = command.ToFailedCommandResponse(
                new Reason
                {
                    Code = ReasonCodes.COMMAND_NOT_ALLOWED,
                    Description = "This command is not allowed"
                });
            return CommandSender.SendCommandAsync(responseCommand);
        }
    }
}