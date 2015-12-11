using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Senders
{
    public interface ICommandSender
    {
        Task SendCommandAsync(Command command);
    }
}