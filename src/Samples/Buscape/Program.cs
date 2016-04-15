//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Takenet.MessagingHub.Client;

//namespace Buscape
//{
//    class Program
//    {
//        // Go to http://messaginghub.io to register your application and get your access key
//        private const string Account = "buscape";
//        private const string AccessKey = "Mkh5UndV";

//        private static void Main()
//        {
//            MainAsync().Wait();
//        }

//        private static async Task MainAsync()
//        {
//            var client = new MessagingHubClientBuilder()
//                .UsingHostName("hmg.msging.net")
//                .UsingAccessKey(Account, AccessKey)
//                .Build();

//            var settings = new Dictionary<string, object> { { "buscapeAppToken", "464f6e73625a4a5a5650593d" } };

//            using (
//                var plainTextMessageReceiver =
//                    new PlainTextMessageReceiver(settings))
//            {
//                client.AddMessageReceiver(plainTextMessageReceiver);

//                await client.StartAsync();

//                Console.WriteLine("Press any key to exit");
//                Console.ReadKey();

//                await client.StopAsync();
//            }
//        }
//    }
//}
