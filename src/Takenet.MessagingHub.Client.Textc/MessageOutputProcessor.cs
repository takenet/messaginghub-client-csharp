using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using Takenet.Textc;
using Takenet.Textc.Processors;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class MessageOutputProcessor : IOutputProcessor
    {
        private readonly IMessagingHubSender _sender;

        public MessageOutputProcessor(IMessagingHubSender sender)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            _sender = sender;
        }

        public Task ProcessOutputAsync(object output, IRequestContext context, CancellationToken cancellationToken)
        {
            if (output == null) return Task.CompletedTask;

            var to = context.GetMessagePp() ?? context.GetMessageFrom();
            if (to == null)  throw new ArgumentException("Could not determine the message sender", nameof(context));

            var document = output as Document;
            if (document != null) return _sender.SendMessageAsync(document, to, cancellationToken);

            return _sender.SendMessageAsync(output.ToString(), to, cancellationToken);
        }
    }
}
