using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Receivers
{
    /// <summary>
    /// Command receiver that automatically respond to any command as an unsupported command
    /// </summary>
    internal class UnsupportedCommandReceiver : EnvelopeReceiverBase, ICommandReceiver
    {
        public Task ReceiveAsync(Command command)
        {
            var responseCommand = command.ToFailedCommandResponse(
                new Reason
                {
                    Code = ReasonCodes.COMMAND_NOT_ALLOWED,
                    Description = "This command is not allowed"
                });
            return CommandSender.SendAsync(responseCommand);
        }
    }
}