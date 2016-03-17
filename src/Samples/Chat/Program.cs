using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;

namespace Chat
{
    class Program
    {
        private const string Login = "";
        private const string AccessKey = "";
        

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var client = new MessagingHubClientBuilder()
                .UsingAccessKey(Login, AccessKey)
                .AddMessageReceiver(new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText)
                .Build();

            await client.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            await client.StopAsync();
        }
    }
}
