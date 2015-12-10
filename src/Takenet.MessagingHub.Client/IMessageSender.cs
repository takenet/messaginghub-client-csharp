using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IMessageSender
    {
        Task SendMessageAsync(Message message);
    }
}
