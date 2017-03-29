using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;

namespace Takenet.MessagingHub.Client.Extensions.ArtificialIntelligence
{
    public interface ITalkServiceExtension
    {
        Task<Analysis> AnalysisAsync(string sentence, CancellationToken cancellationToken);
    }
}
