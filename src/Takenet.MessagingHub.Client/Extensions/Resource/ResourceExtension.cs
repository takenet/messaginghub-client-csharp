using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Resource
{
    public class ResourceExtension : BucketExtension, IResourceExtension
    {
        public ResourceExtension(IMessagingHubSender sender)
            : base(sender, "resources")
        {

        }
    }
}
