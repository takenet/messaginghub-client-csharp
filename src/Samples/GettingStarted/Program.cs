using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace GettingStarted
{
    class Program
    {
        private static IMessagingHubClient _client;
        // Go to http://messaginghub.io to register your application and get your access key
        private const string Account = "getting-started";
        private const string AccessKey = "NnliTHNE";

        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            _client = new MessagingHubClientBuilder()
                            .UsingAccessKey(Account, AccessKey)
                            .Build();

            _client.AddMessageReceiver(ReceiveAsync);
            await _client.StartAsync();

            await _client.SendMessageAsync("Hi, you are now connected to the Messaging Hub!", Account);
            await Task.Delay(1000);

            await _client.StopAsync();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            // Text messages sent to your application will be received here
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine($"From: {message.From}");
            Console.WriteLine($"At: {DateTime.Now}");
            Console.WriteLine($"With: \"{message.Content}\"");
            Console.WriteLine();

            const string ItWorks = "It works!";

            if (message.Content.ToString() != ItWorks)
                await _client.SendMessageAsync(ItWorks, message.From, cancellationToken);
        }
    }
}
