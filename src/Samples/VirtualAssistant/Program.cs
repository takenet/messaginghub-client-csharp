using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.Textc;
using Takenet.Textc.Csdl;

namespace Takenet.Calendar
{
    class Program
    {
        private const string Login = "calendar";
        private const string AccessKey = "MnE1Mmlm";

        private static CultureInfo EnUsCulture = CultureInfo.GetCultureInfo("en-US");
        private static CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

        private static CalendarImpl _calendar;


        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            _calendar = new CalendarImpl();

            // Adds the calendar syntaxes and actions for en-us and pt-br
            

            var client = new MessagingHubClient()
                .UsingAccessKey(Login, AccessKey)
                .NewTextcMessageReceiverBuilder()
                .ForSyntaxes(
                    "^[:Word?(hey,ok) :LDWord?(calendar,agenda) :Word?(add,new,create) command:LDWord(remind,reminder) :Word?(me) :Word~(to,of) message:Text :Word?(for) when:LDWord?(today,tomorrow,someday)]",
                    "^[:Word?(ola,oi,ok) :LDWord?(calendario,agenda) :Word?(adicione,novo,crie) :Word?(um,me) command:LDWord(lembrete,lembre) :Word~(para,da,de,do) message:Text when:LDWord?(hoje,amanha,eventualmente)]")
                .Return<string, string, IRequestContext, string>((text, when, context) =>
                {
                    var reminder = _calendar.AddReminder(text, CalendarImpl.ParseWhen(when), context);
                    return Task.FromResult(context.Culture.Equals(EnUsCulture)
                        ? $"Reminder '{reminder.Text}' added successfully for '{reminder.When.ToString(EnUsCulture)}'"
                        : $"Lembrete '{reminder.Text}' adicionado com sucesso para '{reminder.When.ToString(PtBrCulture)}'");
                })
                .ForSyntaxes(
                    "^[:Word?(hey,ok) :LDWord?(calendar,agenda) :Word?(add,new,create) command+:LDWord(remind,reminder) :Word?(for,me) when+:LDWord?(today,tomorrow,someday)]",
                    "^[:Word?(ola,oi,ok) :LDWord?(calendario,agenda) :Word?(adicione,novo,crie) :Word?(um,me) command:LDWord(lembrete,lembre) :Word?(para,da,de,do) when+:LDWord?(hoje,amanha,eventualmente)]")
                .Return<string, IRequestContext, string>((when, context) =>
                    Task.FromResult(context.Culture.Equals(EnUsCulture)
                        ? $"What do you want to be reminded {when}?"
                        : $"De quê você deseja ser lembrado {when}?"))
                .ForSyntaxes(
                    "[when:LDWord?(today,tomorrow,someday) :LDWord(reminders)]",
                    "[:LDWord(lembretes), :Word?(de,para) when:LDWord?(hoje,amanha,eventualmente)]")
                .Return<string, IRequestContext, string>((when, context) =>
                {
                    DateTime? reminderDate = CalendarImpl.ParseWhen(when);
                    if (reminderDate.Equals(DateTime.MaxValue)) reminderDate = null;

                    var reminders = _calendar.GetReminders(reminderDate);

                    var remindersDictionary = reminders
                        .GroupBy(r => r.When)
                        .ToDictionary(r => r.Key, r => r.Select(reminder => reminder.Text));

                    var messageBuilder = new StringBuilder();

                    foreach (var date in remindersDictionary.Keys)
                    {
                        messageBuilder.AppendLine($"Reminders for '{date}':");

                        foreach (var reminderMessage in remindersDictionary[date])
                        {
                            messageBuilder.AppendLine($"* {reminderMessage}");
                        }

                        messageBuilder.AppendLine();
                    }

                    return Task.FromResult(messageBuilder.ToString());
                })
                .BuildAndAddMessageReceiver();

            // Starts the client
            await client.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await client.StopAsync();
            
        }
    }
}
