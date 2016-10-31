using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Common.Resources;

namespace Takenet.MessagingHub.Client.Extensions.EventTracker
{
    public interface IEventTrackExtension
    {
        Task<IEnumerable<EventTrack>> GetAsync(DateTime filterDate, int take = 20, CancellationToken cancellationToken = new CancellationToken());
        Task AddAsync(string eventName, string actionName, CancellationToken cancellationToken = new CancellationToken());
    }
}
