using Lime.Protocol;
using System;
using System.Threading.Tasks;
using System.Linq;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Chat
{
    public class PlainTextMessageReceiver : MessageReceiverBase
    {
        private static string _targetPostmaster = "postmaster@0mn.io/#awirisomni1";
        private static string _targetGroupDomain = "groups.0mn.io";
        private static string _groupPrefix = "chatSC_";

        private static string _helpCommand = "ajuda";
        private static string _helpMessage = "Envie #assunto para entrar em um chat";

        private static char[] _invalidChars = new[] {'#','/','@','ç' };

        private static Identity GroupIdentity(string groupName) => new Identity($"{_groupPrefix}{groupName}", _targetGroupDomain);

        public async override Task ReceiveAsync(Message message)
        {

            if(message.Content == null)
            {
                await NotifyAndExplain(message, _helpMessage);
                return;
            }

            var messageText = message.Content.ToString();
            
            if (!messageText.StartsWith("#") || messageText.Equals(_helpCommand, StringComparison.InvariantCultureIgnoreCase))
            {
                await NotifyAndExplain(message, _helpMessage);
                return;
            }

            string groupName = messageText.Substring(1);

            if (!IsValidGroupName(groupName))
            {
                await NotifyAndExplain(message, $"Formato de assunto inválido. Evite os caracteres {string.Join(" ", _invalidChars)}");
                return;
            }

            var getResponse = await EnvelopeSender.SendCommandAsync(BuildGetGroupCommand(groupName));

            if (getResponse.Status == CommandStatus.Failure)
            {
                if (getResponse.Reason != null && getResponse.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                {
                    var createResponse = await EnvelopeSender.SendCommandAsync(BuildCreateGroupCommand(groupName));

                    if(createResponse.Status == CommandStatus.Failure)
                    {
                        await NotifyAndExplain(message, "Erro ao criar o chat. Tente novamente mais tarde");
                        return;
                    }
                }
                else
                {
                    await NotifyAndExplain(message, "Erro ao verificar existência do chat. Tente novamente mais tarde");
                    return;
                }
            }

            var insertMemberResponse = await EnvelopeSender.SendCommandAsync(BuildInsertMemberCommand(groupName, message.From));

            if(insertMemberResponse.Status == CommandStatus.Failure)
            {
                await NotifyAndExplain(message, "Erro ao inserir usuário no chat. Tente novamente mais tarde");
                return;
            }

            await NotifyAndExplain(message, $"Você agora faz parte do chat #{groupName}");
        }
        
        private async Task NotifyAndExplain(Message message, string helpMessage)
        {
            await SendConsumedNotification(message);
            await EnvelopeSender.SendMessageAsync(helpMessage, message.From);
        }

        private async Task SendConsumedNotification(Message message)
        {
            var sender = message.Pp ?? message.From;

            await EnvelopeSender.SendNotificationAsync(new Notification { Id = message.Id, To = sender, Event = Event.Consumed  });
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

        private Command BuildInsertMemberCommand(string groupName, Node member)
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

        private Command BuildGetGroupCommand(string groupName)
        {
            return new Command
            {
                Method = CommandMethod.Get,
                To = Node.Parse(_targetPostmaster),
                Uri = new LimeUri($"/groups/{GroupIdentity(groupName)}"),
            };
        }

        private static bool IsValidGroupName(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return false;
            }

            return groupName.IndexOfAny(_invalidChars) == -1;
        }
    }
}
