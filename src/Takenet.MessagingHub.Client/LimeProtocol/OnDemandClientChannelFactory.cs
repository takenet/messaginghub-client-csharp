using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal class OnDemandClientChannelFactory : IOnDemandClientChannelFactory
    {
        public IOnDemandClientChannel Create(IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            return new OnDemandClientChannel(establishedClientChannelBuilder);
        }
    }
}
