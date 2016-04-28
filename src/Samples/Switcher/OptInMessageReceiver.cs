using Lime.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace Switcher
{
    public class OptInMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public OptInMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())        
        {
            var senderAddress = envelope.From;
            if (GetPhoneNumberDomains().Contains(senderAddress.Domain))
            {
                var identities = GetPhoneNumberDomains().Select(d => new Identity(senderAddress.Name, d));

                foreach (var identity in identities)
                {
                    var addContactRequest = new Command()
                    {
                        Method = CommandMethod.Set,
                        Uri = new LimeUri(UriTemplates.CONTACTS),
                        Resource = new Contact()
                        {
                            Priority = GetDomainPriority(identity.Domain),
                            Identity = identity
                        }
                    };

                    var addContactResponse = await _sender.SendCommandAsync(addContactRequest, cancellationToken);
                    if (addContactResponse.Status != CommandStatus.Success)
                    {
                        await _sender.SendMessageAsync($"An error occurred while adding a contact with address '{identity}': {addContactResponse.Resource}",
                            senderAddress, cancellationToken);
                        return;
                    }

                    // Link it to the other domains identities
                    foreach (var linkedIdentity in identities.Where(i => !i.Equals(identity)))
                    {
                        var linkContactRequest = new Command()
                        {
                            Method = CommandMethod.Set,
                            Uri = new LimeUri($"/contacts/{identity}/linked"),
                            Resource = new Contact()
                            {
                                Identity = linkedIdentity
                            }
                        };

                        var linkContactResponse = await _sender.SendCommandAsync(linkContactRequest, cancellationToken);
                        if (linkContactResponse.Status != CommandStatus.Success)
                        {
                            await _sender.SendMessageAsync($"An error occurred while linking the contact '{identity}' to '{linkedIdentity}': {linkContactResponse.Resource}",
                                senderAddress, cancellationToken);
                            return;
                        }
                    }                    
                }
                Startup.Destinations.Add(senderAddress.Name);
                await _sender.SendMessageAsync($"Done! The contacts {identities.Select(i => i.ToString()).Aggregate((a, b) => $"{a}, {b}").Trim(' ')} are now linked.",
                    senderAddress, cancellationToken);
            }
            else
            {
                await _sender.SendMessageAsync("It seems your identifier is not a valid phone number so I cannot subscribe you :(",
                    senderAddress, cancellationToken);
            }
        }

        private static int GetDomainPriority(string domain)
        {
            switch (domain)
            {
                case "0mn.io":
                    return 1;
                case "tangram.com.br":
                    return 2;
                default:
                    return 100;
            }            
        }

        private static IEnumerable<string> GetPhoneNumberDomains()
        {
            yield return "0mn.io";
            yield return "tangram.com.br";
            yield return "msging.net";
        }        
    }
}
