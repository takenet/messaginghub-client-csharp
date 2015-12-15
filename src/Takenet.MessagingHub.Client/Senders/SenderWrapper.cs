using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Client;
using System;
using Takenet.MessagingHub.Client.Lime;

namespace Takenet.MessagingHub.Client.Senders
{
    class SenderWrapper : IMessageSender, ICommandSender, INotificationSender
    {
        IClientChannel _clientChannel;
        IEnvelopeProcessor<Command> _commandProcessor;
        TimeSpan _timeout;

        public SenderWrapper(IClientChannel clientChannel, IEnvelopeProcessor<Command> commandProcessor, TimeSpan timeout)
        {
            _clientChannel = clientChannel;
            _commandProcessor = commandProcessor;
            _timeout = timeout;
        }

        public Task SendMessageAsync(Message message) => _clientChannel.SendMessageAsync(message);

        public Task<Command> SendCommandAsync(Command command) => _commandProcessor.SendReceiveAsync(command, _timeout);

        public Task SendNotificationAsync(Notification notification) => _clientChannel.SendNotificationAsync(notification);
    }
}
