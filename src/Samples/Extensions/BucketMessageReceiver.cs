using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace Extensions
{
    public class BucketMessageReceiver : IMessageReceiver
    {
        private readonly IBucketExtension _bucketExtension;
        private readonly IMessagingHubSender _sender;

        public BucketMessageReceiver(IMessagingHubSender sender, IBucketExtension bucketExtension)
        {
            _bucketExtension = bucketExtension;
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Store last access date
            var jsonLastAccess = new JsonDocument();
            jsonLastAccess.Add("lastAccessDate", DateTimeOffset.Now);

            await _bucketExtension.SetAsync(message.From.ToString(), jsonLastAccess);

            //Get last access date
            await _bucketExtension.GetAsync<JsonDocument>(message.From.ToString());
        }
    }
}
