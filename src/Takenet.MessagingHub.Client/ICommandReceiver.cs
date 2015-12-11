using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface ICommandReceiver
    {
        Task ReceiveAsync(Command command);
    }
}