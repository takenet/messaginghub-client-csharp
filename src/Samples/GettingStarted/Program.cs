using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace GettingStarted
{
    class Program
    {
        // Go to http://messaginghub.io to register your application and get your access key
        private const string Account = "getting-started";
        private const string AccessKey = "NnliTHNE";

        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var client = new MessagingHubClientBuilder()
                            .UsingAccessKey(Account, AccessKey)
                            .Build();

            client.AddMessageReceiver(ReceiveAsync);
            await client.StartAsync();

            await client.SendMessageAsync("Hi, you are now connected to the Messaging Hub!", Account);
            await Task.Delay(1000);

            await client.StopAsync();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            // Text messages sent to your application will be received here
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine($"From: {message.From}");
            Console.WriteLine($"At: {DateTime.Now}");
            Console.WriteLine($"With: \"{message.Content}\"");
            Console.WriteLine();

            const string ItWorks = "It works!";

            if (message.Content.ToString() != ItWorks)
                await sender.SendMessageAsync(ItWorks, message.From, cancellationToken);
        }
    }
}
