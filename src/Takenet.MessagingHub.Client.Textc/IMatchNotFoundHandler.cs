using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public interface IMatchNotFoundHandler
    {
        Task OnMatchNotFoundAsync(Message message, IRequestContext context, CancellationToken cancellationToken);
    }
}