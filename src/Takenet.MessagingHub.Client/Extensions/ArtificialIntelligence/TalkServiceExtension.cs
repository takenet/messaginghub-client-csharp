using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Takenet.Iris.Messaging.Resources;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.ArtificialIntelligence
{
    public class TalkServiceExtension : ExtensionBase, ITalkServiceExtension
    {
        private static readonly Node TalkServiceAddress = Node.Parse($"postmaster@talkservice.{Constants.DEFAULT_DOMAIN}");

        public TalkServiceExtension(IMessagingHubSender sender)
            : base(sender)
        {

        }

        public async Task<Analysis> AnalysisAsync(string sentence, CancellationToken cancellationToken)
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = TalkServiceAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/analysis?sentence={HttpUtility.UrlEncode(sentence)}")
            };

            return await ProcessCommandAsync<Analysis>(requestCommand, cancellationToken);

        }
    }
}
