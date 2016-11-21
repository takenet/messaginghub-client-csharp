using System;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal class OnDemandClientChannelFactory : IOnDemandClientChannelFactory
    {
        public IEstablishedClientChannelBuilder ChannelBuilder { get; set; }

        public OnDemandClientChannelFactory(IEstablishedClientChannelBuilder channelBuilder)
        {
            if (channelBuilder == null) throw new ArgumentNullException(nameof(channelBuilder));

            ChannelBuilder = channelBuilder;
        }

        public IOnDemandClientChannel Create()
        {
            return new MultiplexerClientChannel(ChannelBuilder as EstablishedClientChannelBuilder);
        }
    }
}
