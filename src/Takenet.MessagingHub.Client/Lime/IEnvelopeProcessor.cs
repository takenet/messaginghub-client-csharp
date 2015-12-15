using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Lime
{
    interface IEnvelopeProcessor<T> where T : Envelope
    {
        void Start();
        Task StopAsync();
        Task<T> SendReceiveAsync(T envelope, TimeSpan timeout);
    }
}
