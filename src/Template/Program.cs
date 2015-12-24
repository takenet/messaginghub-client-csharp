using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Template
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Init().Wait();
        }

        private class MessageReceiver : MessageReceiverBase
        {
            public override async Task ReceiveAsync(Message message)
            {
                // Text messages sent to your application will be received here
                await MessageSender.SendMessageAsync("It works!", message.From);
            }
        }


        private static async Task Init()
        {
            // Go to http://console.messaginghub.io to register your application and get your access key

            const string login = "yourApplicationName";
            const string accessKey = "yourAccessKey";

            // Instantiates a MessageHubClient using its fluent API
            // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
            var client = new MessagingHubClient() 
                            .UsingAccessKey(login, accessKey)
                            .AddMessageReceiver(new MessageReceiver(), MediaTypes.PlainText);

            // Starts the client
            await client.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await client.StopAsync();
        }
    }
}
