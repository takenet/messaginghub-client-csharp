using System;
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
            => sender.SendMessageAsync(CreatePlainTextContent(content), to);

        public static Task SendMessageAsync(this IMessageSender sender, Document content, string to)
        => sender.SendMessageAsync(content, Node.Parse(to));

        public static Task SendMessageAsync(this IMessageSender sender, Document content, Node to)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var message = new Message
            {
                To = to,
                Content = content
            };
            return sender.SendMessageAsync(message);
        }

        private static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}