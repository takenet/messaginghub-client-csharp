﻿using Lime.Messaging.Contents;
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

        static PlainText CreatePlainTextContent(string content) => new PlainText { Text = content };
    }
}
