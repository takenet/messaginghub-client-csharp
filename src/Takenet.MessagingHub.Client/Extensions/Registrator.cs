using Lime.Protocol.Serialization;
using Takenet.MessagingHub.Client.Extensions.Session;

namespace Takenet.MessagingHub.Client.Extensions
{
    public static class Registrator
    {
        public static void RegisterDocuments()
        {
            TypeUtil.RegisterDocument<NavigationSession>();
            TypeUtil.RegisterDocument<StateDocument>();
        }
    }
}
