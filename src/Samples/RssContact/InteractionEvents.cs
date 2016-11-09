using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssContact
{
    public static class InteractionEvents
    {
        public static readonly InteractionEvent MessageReceived = new InteractionEvent("Message", "Received");
        public static readonly InteractionEvent MessageReceivedSuccess = new InteractionEvent("Message", "ReceivedSuccess");
        public static readonly InteractionEvent MessageReceivedFailed = new InteractionEvent("Message", "ReceivedFailed");
        public static readonly InteractionEvent MessageSent = new InteractionEvent("Message", "Sent");
    }

    public class InteractionEvent
    {
        public readonly string Category;
        public readonly string Action;

        public InteractionEvent(string category, string action)
        {
            Category = category;
            Action = action;
        }
    }
}
