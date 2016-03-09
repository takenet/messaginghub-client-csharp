using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;

namespace $rootnamespace$
{
    class Program
    {
        private const string Login = "<yout login>";
        private const string AccessKey = "<yout access key>";

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
