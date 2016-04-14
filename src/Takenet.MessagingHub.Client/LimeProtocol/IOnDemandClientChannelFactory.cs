using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal interface IOnDemandClientChannelFactory
    {
        IEstablishedClientChannelBuilder ChannelBuilder { get; set; }

        IOnDemandClientChannel Create();
    }
}
