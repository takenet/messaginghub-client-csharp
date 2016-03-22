using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;

namespace Reminder
{
    public class ReminderProcessor
    {
        private readonly List<Reminder> _reminders;

        public ReminderProcessor()
        {
            _reminders = new List<Reminder>();
        }

        public Task<string> AddReminderAsync(string text, string when, IRequestContext context)
        {
            var whenDateTime = ParseWhen(when);

            var reminder = new Reminder(text, whenDateTime);
            _reminders.Add(reminder);
            context.RemoveVariable(nameof(when));
            return $"Lembrete '{reminder.Text}' adicionado com sucesso para '{reminder.When}'".AsCompletedTask();
        }

        public Task<string> GetRemindersAsync(string when)
        {
            var reminders = string.IsNullOrEmpty(when) ? 
                _reminders : 
                _reminders.Where(r => r.When.Date.Equals(ParseWhen(when)));

            if (!reminders.Any())
            {
                return !string.IsNullOrWhiteSpace(when) ? 
                    $"Não há lembretes definidos para {when}".AsCompletedTask() : 
                    "Não há lembretes definidos".AsCompletedTask();
            }

            var remindersDictionary = reminders
                .GroupBy(r => r.When)
                .ToDictionary(r => r.Key, r => r.Select(reminder => reminder.Text));

            var messageBuilder = new StringBuilder();

            foreach (var date in remindersDictionary.Keys)
            {
                messageBuilder.AppendLine($"Lembretes para '{date}':");
                foreach (var reminderMessage in remindersDictionary[date])
                {
                    messageBuilder.AppendLine($"* {reminderMessage}");
                }

                messageBuilder.AppendLine();
            }

            return Task.FromResult(messageBuilder.ToString());
        }

        public class Reminder
        {
            public Reminder(string text, DateTime when)
            {
                Text = text;
                When = when;
            }

            public string Text { get; }

            public DateTime When { get; }
        }

        public static DateTime ParseWhen(string when)
        {
            switch (when)
            {
                case "yesterday":
                case "ontem":
                    return DateTimeOffset.UtcNow.AddDays(-1).Date;

                case "today":
                case "hoje":
                    return DateTimeOffset.UtcNow.Date;

                case "tomorrow":
                case "amanha":
                case "amanhã":
                    return DateTimeOffset.UtcNow.AddDays(1).Date;

                default:
                    return DateTime.MaxValue;

            }
        }
    }
}
