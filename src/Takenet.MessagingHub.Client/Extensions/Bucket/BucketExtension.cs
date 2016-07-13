using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Bucket
{
    public sealed class BucketExtension : IBucketExtension
    {
        private readonly IMessagingHubSender _messagingHubSender;

        public BucketExtension(IMessagingHubSender messagingHubSender)
        {
            _messagingHubSender = messagingHubSender;
        }

        public async Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default(CancellationToken)) where T : Document
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var getRequestCommand = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/buckets/{id}")
            };

            var getResponseCommand = await _messagingHubSender.SendCommandAsync(
                getRequestCommand,
                cancellationToken);

            if (getResponseCommand.Status != CommandStatus.Success)
            {
                if (getResponseCommand.Reason?.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                {
                    return null;
                }

                throw new LimeException(
                    getResponseCommand.Reason ?? 
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
            return (T)getResponseCommand.Resource;
        }

        public async Task SetAsync<T>(string id, T document, TimeSpan expiration = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) where T : Document
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var uri = $"/buckets/{id}";
            if (expiration != default(TimeSpan))
            {
                uri = $"{uri}?expiration={expiration.TotalMilliseconds}";
            }

            var setRequestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(uri),
                Resource = document
            };

            var setResponseCommand = await _messagingHubSender.SendCommandAsync(
                setRequestCommand,
                cancellationToken);
            if (setResponseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    setResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var deleteRequestCommand = new Command()
            {
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/buckets/{id}")
            };

            var deleteResponseCommand = await _messagingHubSender.SendCommandAsync(
                deleteRequestCommand,
                cancellationToken);

            if (deleteResponseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    deleteResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }
    }
}