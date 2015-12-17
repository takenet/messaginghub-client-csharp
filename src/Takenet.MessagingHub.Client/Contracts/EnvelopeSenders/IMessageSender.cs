using Lime.Protocol;
using Takenet.MessagingHub.Client.Receivers;

namespace Takenet.MessagingHub.Client.Senders
{
    /// <summary>
    /// Proxy used to send messages to the Messaging Hub
    /// </summary>
    public interface IMessageSender : IEnvelopeSender<Message>
    {
    }
}
