using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client
{
    public interface IStartable
    {
        Task StartAsync();
    }
}