using Lime.Messaging.Contents;
using Lime.Protocol;
using System.Threading.Tasks;

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
                To = message.From,
                Id = message.Id,
                Event = @event
            };
            return notification;
        }

        static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}
