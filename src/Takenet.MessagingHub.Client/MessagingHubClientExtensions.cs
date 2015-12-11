using Lime.Messaging.Contents;
using Lime.Protocol;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    public static class MessagingHubClientExtensions
    {
        public static Task SendMessageAsync(this IMessageSender sender, string content, string to)
        {
            return sender.SendMessageAsync(content, Node.Parse(to));
        }

        public static Task SendMessageAsync(this IMessageSender sender, string content, Node to)
        {
            var message = new Message
            {
                To = to,
                Content = CreatePlainTextContent(content)
            };
            return sender.SendMessageAsync(message);
        }

        public static Task SendMessageAsync(this IMessagingHubClient client, string content, string to) => client.MessageSender.SendMessageAsync(content, to);

        public static Notification ToReceivedNotification(this Message message) => message.ToNotification(Event.Received);

        public static Notification ToConsumedNotification(this Message message) => message.ToNotification(Event.Consumed);

        public static Notification ToFailedNotification(this Message message, Reason reason)
        {
            var notification = message.ToNotification(Event.Failed);
            notification.Reason = reason;
            return notification;
        }

        public static Notification ToNotification(this Message message, Event @event)
        {
            var notification = new Notification
            {
                Id = message.Id,
                To = message.From,
                Event = @event
            };
            return notification;
        }

        public static Command ToFailedCommandResponse(this Command command, Reason reason)
        {
            var responseCommand = command.ToCommandResponse(CommandStatus.Failure);
            responseCommand.Reason = reason;
            return responseCommand;
        }

        public static Command ToCommandResponse(this Command command, CommandStatus status)
        {
            var responseCommand = new Command
            {
                Id = command.Id,
                To = command.From,
                Method = command.Method,
                Status = status
            };
            return responseCommand;
        }

        static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}
