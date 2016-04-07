using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Deprecated
{
    [Obsolete]
    public interface IStartable
    {
        Task StartAsync();
    }
}