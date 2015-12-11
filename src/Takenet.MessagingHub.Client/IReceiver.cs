using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IReceiver<T>
        where T : Envelope
    {
        Task ReceiveAsync(T envelope);
    }
}