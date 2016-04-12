using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Chat
{
    public class ChatMessageReceiver : MessageReceiverBase
    {
        private const string _targetPostmaster = "postmaster@0mn.io/#irisomni1";
        private const string _targetGroupDomain = "groups.0mn.io";
        private const string _groupPrefix = "omniChat_";

        private const string _helpCommand = "ajuda";
        private const string _listCommand = "listar";
        private const string _helpMessage = "Envie # + tema para entrar em um grupo ou listar para exibir os grupos já existentes";

        private static Identity GroupIdentity(string groupName) => new Identity($"{_groupPrefix}{groupName}", _targetGroupDomain);

        public override async Task ReceiveAsync(MessagingHubSender sender, Message message, CancellationToken token)
        {
            if(message.To.Domain.Equals(_targetGroupDomain, StringComparison.InvariantCultureIgnoreCase))
                return;

            if(message.Content == null)
            {
                await sender.SendMessageAsync(_helpMessage, message.From, token);
                return;
            }

            var messageText = message.Content.ToString();
            
            if(messageText.Equals(_listCommand, StringComparison.InvariantCultureIgnoreCase))
            {
                var groups = await ListGroupsAsync(sender, token);

                await sender.SendMessageAsync(_helpMessage, groups, token);
                return;
            }

            if (!messageText.StartsWith("#") || messageText.Equals(_helpCommand, StringComparison.InvariantCultureIgnoreCase))
            {
                await sender.SendMessageAsync(_helpMessage, message.From, token);
                return;
            }

            var groupName = messageText.Substring(1);

            if (!IsValidGroupName(groupName))
            {
                await sender.SendMessageAsync("Formato de assunto inválido. Evite caracteres especiais e acentuação", message.From, token);
                return;
            }

            var getResponse = await sender.SendCommandAsync(BuildGetGroupCommand(groupName), token);

            if (getResponse.Status == CommandStatus.Failure)
            {
                if (getResponse.Reason != null && getResponse.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                {
                    var createResponse = await sender.SendCommandAsync(BuildCreateGroupCommand(groupName), token);

                    if(createResponse.Status == CommandStatus.Failure)
                    {
                        await sender.SendMessageAsync("Erro ao criar o grupo. Tente novamente mais tarde", message.From, token);
                        return;
                    }
                }
                else
                {
                    await sender.SendMessageAsync("Erro ao verificar existência do grupo. Tente novamente mais tarde", message.From, token);
                    return;
                }
            }

            var insertMemberResponse = await sender.SendCommandAsync(BuildInsertMemberCommand(groupName, message.From), token);

            if(insertMemberResponse.Status == CommandStatus.Failure)
            {
                await sender.SendMessageAsync("Erro ao inserir usuário no grupo. Tente novamente mais tarde", message.From, token);
                return;
            }

            await sender.SendMessageAsync($"Você foi inserido no grupo #{groupName}", message.From, token);
        }
        
        private static async Task<string> ListGroupsAsync(MessagingHubSender sender, CancellationToken token)
        {
            var listResponse = await sender.SendCommandAsync(BuildListGroupCommand(), token);

            if(listResponse.Status == CommandStatus.Failure)
            {
                return string.Empty;
            }
            
            var documents = (DocumentCollection)listResponse.Resource;

            var stringBuilder = new StringBuilder();
            var groupsPerLine = 1;
            var counter = 0;

            foreach(var document in documents.Items.Take(50))
            {
                if (counter == groupsPerLine)
                {
                    stringBuilder.AppendLine();
                    counter = 0;
                }

                var group = (Lime.Messaging.Resources.Group)document;

                stringBuilder.Append($"#{group.Name} ");
                counter++;
            }

            return stringBuilder.ToString();
        }

        private static Command BuildCreateGroupCommand(string groupName)
        {
            return new Command
            {
                Method = CommandMethod.Set,
                To = Node.Parse(_targetPostmaster),
                Uri = new LimeUri("/groups"),
                Resource = new Lime.Messaging.Resources.Group
                {
                    Type = Lime.Messaging.Resources.GroupType.Public,
                    Identity = GroupIdentity(groupName),
                    Name = groupName
                }
            };
        }

        private static Command BuildInsertMemberCommand(string groupName, Node member)
        {
            return new Command
            {
                Method = CommandMethod.Set,
                To = Node.Parse(_targetPostmaster),
                Uri = new LimeUri($"/groups/{GroupIdentity(groupName)}/members"),
                Resource = new Lime.Messaging.Resources.GroupMember
                {
                    Address = new Node(member.Name, member.Domain, null),
                    Role = Lime.Messaging.Resources.GroupMemberRole.Member
                }
            };
        }

        private static Command BuildGetGroupCommand(string groupName)
        {
            return new Command
            {
                Method = CommandMethod.Get,
                To = Node.Parse(_targetPostmaster),
                Uri = new LimeUri($"/groups/{GroupIdentity(groupName)}"),
            };
        }

        private static Command BuildListGroupCommand()
        {
            return new Command
            {
                Method = CommandMethod.Get,
                To = Node.Parse(_targetPostmaster),
                Uri = new LimeUri($"/groups?role=owner"),
            };
        }

        private static bool IsValidGroupName(string groupName)
        {
            try
            {
                new LimeUri($"{groupName}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
