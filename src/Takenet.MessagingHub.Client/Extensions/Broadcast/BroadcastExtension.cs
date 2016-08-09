using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Extensions.Broadcast
{
    public class BroadcastExtension : ExtensionBase, IBroadcastExtension
    {
        private static readonly Node DistributionListAddress = Node.Parse($"postmaster@broadcast.{Constants.DEFAULT_DOMAIN}");
        private static readonly MediaType DistributionListMediaType = MediaType.Parse("application/vnd.iris.distribution-list+json");
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastExtension"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public BroadcastExtension(IMessagingHubSender sender)
            : base(sender)
        {
            
        }

        public async Task CreateDistributionListAsync(string listName, CancellationToken cancellationToken = default(CancellationToken))
        {            
            var listIdentity = GetListIdentity(listName);

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri("/lists"),
                Resource = new JsonDocument(DistributionListMediaType)
                {
                    {"identity", listIdentity.ToString()}
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task DeleteDistributionListAsync(string listName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/lists/{listIdentity}")
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task AddRecipientAsync(string listName, Identity recipientIdentity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            if (recipientIdentity == null) throw new ArgumentNullException(nameof(recipientIdentity));

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/lists/{listIdentity}/recipients"),
                Resource = new IdentityDocument()
                {
                    Value = recipientIdentity
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task DeleteRecipientAsync(string listName, Identity recipientIdentity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            if (recipientIdentity == null) throw new ArgumentNullException(nameof(recipientIdentity));

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/lists/{listIdentity}/recipients/{recipientIdentity}")
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Identity GetListIdentity(string listName)
        {
            if (string.IsNullOrWhiteSpace(listName))
            {
                throw new ArgumentException("The list name cannot be null or whitespace.", nameof(listName));
            }
            return new Identity(listName, DistributionListAddress.Domain);
        }

        public Task SendMessageAsync(string listName, Document content, CancellationToken cancellationToken = new CancellationToken())
        {
            var message = new Message()
            {
                To = GetListIdentity(listName).ToNode(),
                Content = content
            };

            return Sender.SendMessageAsync(message, cancellationToken);
        }
    }
}