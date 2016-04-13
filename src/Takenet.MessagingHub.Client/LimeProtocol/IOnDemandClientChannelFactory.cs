using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.LimeProtocol
{
    internal interface IOnDemandClientChannelFactory
    {
        IOnDemandClientChannel Create();
    }
}
