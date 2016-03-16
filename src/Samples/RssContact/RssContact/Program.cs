using System;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;

namespace RssContact
{
    class Program
    {
        private const string ApplicationIdentifier = "rss-contact";
        private const string AccessKey = "blB4bUpC";

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var client = new MessagingHubClientBuilder()
                .UsingAccessKey(ApplicationIdentifier, AccessKey)
                .AddMessageReceiver(new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText)
                .Build();

            await client.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            await client.StopAsync();
        }
    }
}
