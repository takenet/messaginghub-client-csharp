using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IStoppable
    {
        Task StopAsync();
    }
}
