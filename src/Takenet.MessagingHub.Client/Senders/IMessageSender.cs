using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Senders
{
    public interface IMessageSender
    {
        Task SendMessageAsync(Message message);
    }
}
