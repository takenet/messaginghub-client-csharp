using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Receivers
{
    public interface IReceiver<T>
        where T : Envelope
    {
        Task ReceiveAsync(T envelope);
    }
}