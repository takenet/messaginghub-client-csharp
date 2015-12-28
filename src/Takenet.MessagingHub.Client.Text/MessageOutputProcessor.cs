using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Text;
using Takenet.Text.Processors;

namespace Takenet.MessagingHub.Client.Text
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
                (context.GetVariable(TextMessageReceiver.PP_VARIABLE_NAME) ??
                 context.GetVariable(TextMessageReceiver.FROM_VARIABLE_NAME)) as Node;

            if (to == null)
            {
                throw new InvalidOperationException("Could not determine the message sender");
            }

            return _client.SendMessageAsync(output.ToString(), to);
        }
    }
}
