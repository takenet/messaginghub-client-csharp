using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client
{
    public interface IMessagingHubClient : IMessagingHubSender, IMessagingHubListener
    {
    }
}