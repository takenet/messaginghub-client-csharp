using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Listener
{
    public interface ICommandReceiver : IEnvelopeReceiver<Command>
    {
    }
}
