using Lime.Protocol;
using Lime.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.EventTracker
{
    public class EventTrackExtension : ExtensionBase, IEventTrackExtension
    {
        const string EVENTRACK_URI = "/event-track";


        public EventTrackExtension(IMessagingHubSender sender)
            : base(sender)
        {
            TypeUtil.RegisterDocument<EventTrack>();
        }

        public async Task AddAsync(string categoryName, string actionName, CancellationToken cancellationToken = new CancellationToken())
        {
            if (string.IsNullOrEmpty(categoryName)) throw new ArgumentNullException(nameof(categoryName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));


            var requestCommand = new Command
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(EVENTRACK_URI),
                Resource = new EventTrack
                {
                    Category = categoryName,
                    Action = actionName,
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
