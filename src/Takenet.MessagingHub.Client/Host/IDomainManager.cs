using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Host
{
    public interface IDomainManager : IDisposable
    {
        AppDomain Domain { get; }
        void Start(string originalPath, string tempPath);
        void Stop();
    }
}
