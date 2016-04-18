using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Switcher
{
    public class OptInMessageReceiver : MessageReceiverBase
    {
        public override async Task ReceiveAsync(Message message)
        {
            var sender = message.GetSender();
            if (GetPhoneNumberDomains().Contains(sender.Domain))
            {
                var identities = GetPhoneNumberDomains().Select(d => new Identity(sender.Name, d));

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

                    var addContactResponse = await EnvelopeSender.SendCommandAsync(addContactRequest);
                    if (addContactResponse.Status != CommandStatus.Success)
                    {
                        await EnvelopeSender.SendMessageAsync($"An error occurred while adding a contact with address '{identity}': {addContactResponse.Resource}",
                            sender);
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

                        var linkContactResponse = await EnvelopeSender.SendCommandAsync(linkContactRequest);
                        if (linkContactResponse.Status != CommandStatus.Success)
                        {
                            await EnvelopeSender.SendMessageAsync($"An error occurred while linking the contact '{identity}' to '{linkedIdentity}': {linkContactResponse.Resource}",
                                sender);
                            return;
                        }
                    }                    
                }
                Startup.Destinations.Add(sender.ToIdentity());
                await EnvelopeSender.SendMessageAsync($"Done! The contacts {identities.Select(i => i.ToString()).Aggregate((a, b) => $"{a}, {b}").Trim(' ')} are now linked.",
                    sender);
            }
            else
            {
                await EnvelopeSender.SendMessageAsync("It seems your identifier is not a valid phone number so I cannot subscribe you :(",
                    sender);
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
