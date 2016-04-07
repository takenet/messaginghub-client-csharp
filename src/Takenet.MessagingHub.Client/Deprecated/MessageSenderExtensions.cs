using System;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Deprecated
{
    /// <summary>
    /// Extension methods for <see cref="IEnvelopeSender"/>
    /// </summary>
    [Obsolete]
    public static class EnvelopeSenderExtensions
    {
        public static Task SendMessageAsync(this IEnvelopeSender sender, string content, string to)
            => sender.SendMessageAsync(content, Node.Parse(to));

        public static Task SendMessageAsync(this IEnvelopeSender sender, string content, Node to)
            => sender.SendMessageAsync(CreatePlainTextContent(content), to);

        public static Task SendMessageAsync(this IEnvelopeSender sender, Document content, string to)
        => sender.SendMessageAsync(content, Node.Parse(to));

        public static Task SendMessageAsync(this IEnvelopeSender sender, Document content, Node to)
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