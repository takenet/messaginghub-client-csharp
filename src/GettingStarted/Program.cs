using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace GettingStarted
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Init().Wait();
        }

        // Go to http://console.messaginghub.io to register your application and get your access key
        private const string Login = "getting-started";
        private const string AccessKey = "NnliTHNE";

        private class MessageReceiver : MessageReceiverBase
        {
            public override async Task ReceiveAsync(Message message)
            {
                // Text messages sent to your application will be received here
                Console.WriteLine($"MESSAGE RECEIVED");
                Console.WriteLine($"From: {message.From}");
                Console.WriteLine($"At: {DateTime.Now}");
                Console.WriteLine($"With: \"{message.Content}\"");
                Console.WriteLine();
                if (message.From.Name != Login)
                    await MessageSender.SendMessageAsync("It works!", message.From);
            }
        }

        private static async Task Init()
        {
            // Instantiates a MessageHubClient using its fluent API
            // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
            var client = new MessagingHubClient() 
                            .UsingAccessKey(Login, AccessKey)
                            .AddMessageReceiver(new MessageReceiver(), MediaTypes.PlainText);

            // Starts the client
            await client.StartAsync();

            // Send message to itself to show it is indeed connected
            await client.SendMessageAsync("Hi, you are now connected to the Messaging Hub!", Login);

            await Task.Delay(1000);

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await client.StopAsync();
        }
    }
}
