using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal class OnDemandClientChannelFactory : IOnDemandClientChannelFactory
    {
        private readonly IEstablishedClientChannelBuilder _establishedClientChannelBuilder;

        public OnDemandClientChannelFactory(IEstablishedClientChannelBuilder establishedClientChannelBuilder)
        {
            _establishedClientChannelBuilder = establishedClientChannelBuilder;
        }

        public IOnDemandClientChannel Create()
        {
            return new OnDemandClientChannel(_establishedClientChannelBuilder);
        }
    }
}
