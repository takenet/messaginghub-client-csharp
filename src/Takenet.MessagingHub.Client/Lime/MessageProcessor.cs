using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;

namespace Takenet.MessagingHub.Client.Lime
{
    /// <summary>
    /// Send and receive commands
    /// </summary>
    internal class MessageProcessor : EnvelopeProcessor<Message>
    {
        private readonly IClientChannel _clientChannel;

        public MessageProcessor(IClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
        }

        protected override Task<Message> ReceiveAsync(CancellationToken cancellationToken)
        {
            return _clientChannel.ReceiveMessageAsync(cancellationToken);
        }

        protected override Task SendAsync(Message envelope, CancellationToken cancellationToken)
        {
            return _clientChannel.SendMessageAsync(envelope);
        }

        public override Task<Message> SendAsync(Message message, TimeSpan timeout)
        {
            return base.SendAsync(message, timeout);
        }
    }
}
