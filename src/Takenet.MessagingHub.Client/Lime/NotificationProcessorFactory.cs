using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    internal class NotificationProcessorFactory : IEnvelopeProcessorFactory<Notification>
    {
        public IEnvelopeProcessor<Notification> Create(IClientChannel clientChannel)
        {
            return new NotificationProcessor(clientChannel);
        }
    }
}
