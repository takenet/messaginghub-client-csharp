using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class MessageOutputProcessor : IOutputProcessor
    {
        private readonly IMessagingHubSender _messagingHubSender;
        
        public MessageOutputProcessor(IMessagingHubSender messagingHubSender)
        {
            if (messagingHubSender == null) throw new ArgumentNullException(nameof(messagingHubSender));
            _messagingHubSender = messagingHubSender;
        }

        public Task ProcessOutputAsync(object output, IRequestContext context, CancellationToken cancellationToken)
        {            
            cancellationToken.ThrowIfCancellationRequested();            

            var to = context.GetMessagePp() ?? context.GetMessageFrom();

            if (to == null)
            {
                throw new InvalidOperationException("Could not determine the message sender");
            }

            var content = output as Document;
            if (content != null)
            {
                return _messagingHubSender.SendMessageAsync(content, to);
            }
            return _messagingHubSender.SendMessageAsync(output.ToString(), to);
        }
    }
}
