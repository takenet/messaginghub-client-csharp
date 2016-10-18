using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendar
{
    public class CalendarProcessor
    {
        public CalendarProcessor()
        {
            
        }

        public Task<string> AddReminderAsync(string reminder)
            => AddReminderForDateAsync(reminder, "eventualmente");

        public Task<string> AddReminderForDateAsync(string reminder, string date)
            => AddReminderForDateAndTimeAsync(reminder, date, "manha");

        public async Task<string> AddReminderForDateAndTimeAsync(string reminder, string date, string time)
        {
            // TODO: Store the reminder for the specified date/time
            return $"O lembrete '{reminder}' foi adicionado para {date} no período da {time}";
        }
    }
}
