using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.BasicSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();
        }

        static async Task Init()
        {
            var client = new MessagingHubClient()
                            .UsingAccount("andreminelli", "123456")
                            .AddMessageReceiver(new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText)
                            //Is possible use a Func to build a receiver
                            .AddNotificationReceiver(() => new PrintNotificationReceiver());
                            
            await client.StartAsync();

            //Send a get account command to server (Only sample)
            var command = new Command {
                Method = CommandMethod.Get,
                Uri = new LimeUri("/account")
            };

            var responseCommand = await client.CommandSender.SendCommandAsync(command);
            var account = (Account)responseCommand.Resource;

            Console.WriteLine(GetAccountString(account));
            Console.WriteLine("Press any key to stop");
            await WaitKeyAsync();
            await client.StopAsync();
        }

        static string GetAccountString(Account account)
        {
            var builder = new StringBuilder();
            
            builder.AppendLine($"{nameof(account.FullName)}:{account.FullName}");
            builder.AppendLine($"{nameof(account.Email)}:{account.Email}");
            builder.AppendLine($"{nameof(account.PhoneNumber)}:{account.PhoneNumber}");
            builder.AppendLine($"{nameof(account.City)}:{account.City}");
            builder.AppendLine($"{nameof(account.Address)}:{account.Address}");

            return builder.ToString();
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
