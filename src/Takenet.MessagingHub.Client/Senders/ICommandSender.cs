using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Proxy used to send commands to the Messaging Hub
    /// </summary>
    public interface ICommandSender : IEnvelopeSender<Command>
    {
    }
}