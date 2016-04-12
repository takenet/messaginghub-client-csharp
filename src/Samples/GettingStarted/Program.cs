using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
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
            var connection = new MessagingHubConnectionBuilder()
                            .UsingAccessKey(Account, AccessKey)
                            .Build();

            await connection.ConnectAsync();

            var listener = new MessagingHubListener(connection);
            listener.AddMessageReceiver(ReceiveAsync);
            await listener.StartAsync();

            var sender = new MessagingHubSender(connection);
            await sender.SendMessageAsync("Hi, you are now connected to the Messaging Hub!", Account);
            await Task.Delay(1000);

            await listener.StopAsync();
            await connection.DisconnectAsync();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task ReceiveAsync(MessagingHubSender sender, Message message, CancellationToken token)
        {
            // Text messages sent to your application will be received here
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine($"From: {message.From}");
            Console.WriteLine($"At: {DateTime.Now}");
            Console.WriteLine($"With: \"{message.Content}\"");
            Console.WriteLine();

            const string ItWorks = "It works!";

            if (message.Content.ToString() != ItWorks)
                await sender.SendMessageAsync(ItWorks, message.From, token);
        }
    }
}
