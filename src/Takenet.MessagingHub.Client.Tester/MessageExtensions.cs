using System.Linq;
using System.Text.RegularExpressions;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.Client.Tester
{
    public static class MessageExtensions
    {
        public static bool MatchReceiverFilters(this Message message, Application application)
        {
            var filters = application.MessageReceivers.Select(r => r.Content);
            var content = message?.Content?.ToString();
            var result = !string.IsNullOrWhiteSpace(content) && filters.Any(f => Regex.IsMatch(content, f, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            return result;
        }
    }
}