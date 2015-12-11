using System;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.BasicSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MessagingHubClient()
                            .UsingAccount("andreminelli", "123456")
                            .AddMessageReceiver(new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText);
            var execution = client.StartAsync().Result;

            Console.WriteLine("Press any key to stop");
            var endingTask = Task.WhenAny(execution, WaitKeyAsync()).Result;
            if (endingTask != execution)
            {
                 client.StopAsync().Wait();
            }
        }

        static Task<char> WaitKeyAsync()
        {
            var tcs = new TaskCompletionSource<char>();
            Task.Run(() =>
            {
                var key = Console.ReadKey(false);
                tcs.SetResult(key.KeyChar);
            });
            return tcs.Task;
        }
    }
}
