using Lime.Messaging.Resources;
using Lime.Protocol;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Connection;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Playground
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Init().Wait();
        }

        private static async Task Init()
        {
            const string login = "aldo";
            const string password = "123456";

            // Instantiates a MessageHubClient using its fluent API
            var connection =
                new MessagingHubConnectionBuilder()
                    // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
                    .UsingHostName("hmg.msging.net")
                    .UsingAccount(login, password)
                    .NewTextcMessageReceiverBuilder()
                    .ForSyntax(":LDWord(help,wtf,ajuda,comofaz)")
                    .Return(
                        () =>
                            Task.FromResult(
                                "Welcome to the calculator! Try send me some math operations, like 'sum 1 and 2' or '3 times 4' and I'll try help you :)"))
                    .ForSyntaxes(
                        "operation+:Word(sum) a:Integer :Word?(and) b:Integer",
                        "a:Integer :Word(plus,more) b:Integer")
                    .Return<int, int, string>((a, b) => Task.FromResult($"The sum result is {a + b}"))
                    .ForSyntaxes(
                        "operation+:Word(subtract,sub) b:Integer :Word(from) a:Integer",
                        "a:Integer :Word(minus) b:Integer")
                    .Return<int, int, string>((a, b) => Task.FromResult($"The subtraction result is {a - b}"))
                    .ForSyntaxes(
                        "operation+:Word(multiply,mul) a:Integer :Word?(and,by) b:Integer",
                        "a:Integer :Word(times) b:Integer")
                    .Return<int, int, string>((a, b) => Task.FromResult($"The multiplication result is {a * b}"))
                    .ForSyntaxes(
                        "operation+:Word(multiply,mul) a:Integer :Word?(and,by) b:Integer",
                        "a:Integer :Word(times) b:Integer")
                    .Return<int, int, string>((a, b) => Task.FromResult($"The multiplication result is {a * b}"))
                    .BuildAndAddTextcMessageReceiver();


            var listener = new MessagingHubListener(connection);
            listener.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText);
            listener.AddNotificationReceiver(new PrintNotificationReceiver());

            // Starts the client
            await connection.ConnectAsync();

            // Instantiates a command to get the current account form the server
            var command = new Command
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri("/account")
            };

            var sender = new MessagingHubSender(connection);

            // Sends the command and stores its response
            var responseCommand = await sender.SendCommandAsync(command, CancellationToken.None);

            // Extract the account from the command response
            var account = (Account)responseCommand.Resource;

            // Logs the result to the console
            Console.WriteLine(AccountDescriptionFor(account));
            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await connection.DisconnectAsync();
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
