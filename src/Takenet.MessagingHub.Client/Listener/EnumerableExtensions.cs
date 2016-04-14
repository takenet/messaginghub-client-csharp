using System.Collections.Generic;
using System.Linq;

namespace Takenet.MessagingHub.Client.Listener
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Coalesce<T>(this IEnumerable<T> enumerable, IEnumerable<T> alternative)
        {
            if (enumerable != null && enumerable.Any()) return enumerable;
            return alternative;
        }
    }
}