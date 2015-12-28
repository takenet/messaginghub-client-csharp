using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Senders;

namespace Takenet.MessagingHub.Client
{
    /// <summary>
    /// Extension methods for <see cref="IMessageSender"/>
    /// </summary>
    public static class MessageSenderExtensions
    {
        public static Task SendMessageAsync(this IMessageSender sender, string content, string to)
            => sender.SendMessageAsync(content, Node.Parse(to));

        public static Task SendMessageAsync(this IMessageSender sender, string content, Node to)
        {
            var message = new Message
            {
                To = to,
                Content = CreatePlainTextContent(content)
            };
            return sender.SendMessageAsync(message);
        }

        private static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}