using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using SmartFormat;

namespace Takenet.MessagingHub.Client.Extensions.Contacts
{
    public class ContactExtension : ExtensionBase, IContactExtension
    {
        private readonly IMessagingHubSender _messagingHubSender;

        public ContactExtension(IMessagingHubSender messagingHubSender)
            : base(messagingHubSender)
        {
            _messagingHubSender = messagingHubSender;
        }

        public Task<Contact> GetAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateGetCommandRequest(
                Smart.Format(Lime.Messaging.Resources.UriTemplates.CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }));

            return ProcessCommandAsync<Contact>(requestCommand, cancellationToken);
        }

        public Task SetAsync(Identity identity, Contact contact, CancellationToken cancellationToken)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact));
            contact.Identity = identity ?? throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateSetCommandRequest(
                contact,
                Lime.Messaging.Resources.UriTemplates.CONTACTS);

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task DeleteAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var requestCommand = CreateDeleteCommandRequest(
                Smart.Format(Lime.Messaging.Resources.UriTemplates.CONTACT, new { contactIdentity = Uri.EscapeDataString(identity.ToString()) }));

            return ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }
}
