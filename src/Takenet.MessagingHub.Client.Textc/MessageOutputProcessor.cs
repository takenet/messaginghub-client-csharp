using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class MessageOutputProcessor : IOutputProcessor
    {
        private readonly IMessagingHubClient _client;

        public MessageOutputProcessor(IMessagingHubClient client)
        {
            _client = client;
        }

        public Task ProcessOutputAsync(object output, IRequestContext context)
        {
            var to =
                (context.GetVariable(TextcMessageReceiver.PP_VARIABLE_NAME) ??
                 context.GetVariable(TextcMessageReceiver.FROM_VARIABLE_NAME)) as Node;

            if (to == null)
            {
                throw new InvalidOperationException("Could not determine the message sender");
            }

            return _client.SendMessageAsync(output.ToString(), to);
        }
    }
}
