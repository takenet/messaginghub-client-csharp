using Lime.Protocol;
using Lime.Protocol.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Takenet.MessagingHub.Client.Sender;

namespace RssContact
{
    public class ForwardToOperatorChannelModule : IChannelModule<Message>, IChannelModule<Command>
    {
        private readonly IMessagingHubSender _sender;

        public ForwardToOperatorChannelModule(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public void OnStateChanged(SessionState state)
        { }

        public Task<Message> OnReceivingAsync(Message envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Command> OnReceivingAsync(Command envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Command> OnSendingAsync(Command envelope, CancellationToken cancellationToken)
        {
            return Task.FromResult(envelope);
        }

        public Task<Message> OnSendingAsync(Message envelope, CancellationToken cancellationToken)
        {
            return Task.FromResult(envelope);
        }
    }
}
