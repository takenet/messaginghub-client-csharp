using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.Textc;

namespace Takenet.Calendar
{
    public class CalendarImpl
    {
        private readonly List<Reminder> _reminders;

        public CalendarImpl()
        {
            _reminders = new List<Reminder>();
        }

        public Reminder AddReminder(string text, DateTime when, IRequestContext context)
        {
            var reminder = new Reminder(text, when);
            _reminders.Add(reminder);
            context.Clear();
            return reminder;
        }

        public IEnumerable<Reminder> GetReminders(DateTime? when)
        {
            if (!when.HasValue)
            {
                return _reminders;
            }

            return _reminders.Where(r => r.When.Date.Equals(when.Value.Date));
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
                    return DateTimeOffset.UtcNow.AddDays(1).Date;

                default:
                    return DateTime.MaxValue;

            }
        }
    }
}
