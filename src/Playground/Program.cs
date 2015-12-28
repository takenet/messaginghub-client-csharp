using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Playground
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Init().Wait();
        }

        private static async Task Init()
        {
            const string login = "andreminelli";
            const string password = "123456";

            // Instantiates a MessageHubClient using its fluent API
            var client = new MessagingHubClient() // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
                            .UsingAccount(login, password)
                            .AddMessageReceiver(messageReceiver: new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText)
                            .AddNotificationReceiver(receiverBuilder: () => new PrintNotificationReceiver());
                            
            // Starts the client
            await client.StartAsync();

            // Instantiates a command to get the current account form the server
            var command = new Command {
                Method = CommandMethod.Get,
                Uri = new LimeUri("/account")
            };

            // Sends the command and stores its response
            var responseCommand = await client.SendCommandAsync(command);

            // Extract the account from the command response
            var account = (Account)responseCommand.Resource;

            // Logs the result to the console
            Console.WriteLine(AccountDescriptionFor(account));
            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await client.StopAsync();
        }

        private static string AccountDescriptionFor(Account account)
        {
            var builder = new StringBuilder();
            
            builder.AppendLine($"{nameof(account.FullName)}:{account.FullName}");
            builder.AppendLine($"{nameof(account.Email)}:{account.Email}");
            builder.AppendLine($"{nameof(account.PhoneNumber)}:{account.PhoneNumber}");
            builder.AppendLine($"{nameof(account.City)}:{account.City}");
            builder.AppendLine($"{nameof(account.Address)}:{account.Address}");

            return builder.ToString();
        }
    }
}
