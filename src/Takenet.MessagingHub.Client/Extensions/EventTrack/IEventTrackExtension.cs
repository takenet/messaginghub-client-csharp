using System;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Extensions.EventTracker
{
    public interface IEventTrackExtension
    {
        Task AddAsync(string eventName, string actionName, CancellationToken cancellationToken = new CancellationToken());
    }
}
