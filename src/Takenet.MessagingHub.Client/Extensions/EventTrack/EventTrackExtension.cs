using Lime.Protocol;
using Lime.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.Iris.Common.Resources;
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

        public async Task<IEnumerable<EventTrack>> GetAsync(DateTime filterDate, int take = 20, CancellationToken cancellationToken = new CancellationToken())
        {
            var requestCommand = new Command
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"{EVENTRACK_URI}?filterDate={filterDate.ToString("yyyy-MM-dd")}&take={take}"),
            };

            var response = await ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
            return response.Items.Cast<EventTrack>();
        }

        public async Task AddAsync(string eventName, string actionName, CancellationToken cancellationToken = new CancellationToken())
        {
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));


            var requestCommand = new Command
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(EVENTRACK_URI),
                Resource = new EventTrack
                {
                    Action = actionName,
                    Event = eventName
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
