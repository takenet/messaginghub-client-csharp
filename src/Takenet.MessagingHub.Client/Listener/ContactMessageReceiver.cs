using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Messaging.Resources;
using Takenet.MessagingHub.Client.Extensions.Directory;
using Takenet.MessagingHub.Client.Extensions.Contacts;

namespace Takenet.MessagingHub.Client.Listener
{
    /// <summary>
    /// Defines a <see cref="IMessageReceiver"/> that includes contact information about the message sender.
    /// </summary>
    public abstract class ContactMessageReceiver : IMessageReceiver
    {
        private readonly IContactExtension _contactExtension;
        private readonly IDirectoryExtension _directoryExtension;

        protected ContactMessageReceiver(
            IContactExtension contactExtension,
            IDirectoryExtension directoryExtension)
        {
            _directoryExtension = directoryExtension;
            _contactExtension = contactExtension;
        }

        public async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            var identity = envelope.From.ToIdentity();
            var contact = await _contactExtension.GetAsync(identity, cancellationToken);
            if (contact == null)
            {
                contact = new Contact
                {
                    Identity = identity
                };

                var account = await _directoryExtension.GetDirectoryAccountAsync(identity, cancellationToken);
                if (account != null)
                {
                    contact.Name = account.FullName;
                    contact.Address = account.Address;
                    contact.CellPhoneNumber = account.CellPhoneNumber;
                    contact.City = account.City;
                    contact.Culture = account.Culture;
                    contact.Email = account.Email;
                    contact.Extras = account.Extras;
                    contact.Gender = account.Gender;
                    contact.PhoneNumber = account.PhoneNumber;
                    contact.PhotoUri = account.PhotoUri;
                    contact.Timezone = account.Timezone;
                }
            }

            await ReceiveAsync(envelope, contact, cancellationToken);
        }
        /// <summary>
        /// Receives a message with the contact information.
        /// </summary>
        protected abstract Task ReceiveAsync(Message message, Contact contact, CancellationToken cancellationToken = default(CancellationToken));
    }
}
