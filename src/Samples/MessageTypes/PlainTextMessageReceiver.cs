using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public PlainTextMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = "Olá. Estou aqui para testar tipos canônicos de mensagem. Como prefere que eu lhe responda?",
                Options = new []
                {
                    new SelectOption
                    {
                        Order = 1,
                        Text = "Um texto inspirador!"
                    },
                    new SelectOption
                    {
                        Order = 2,
                        Text = "Uma imagem motivacional!"
                    },
                    new SelectOption
                    {
                        Order = 3,
                        Text = "Um link para algo interessante!"
                    },
                    new SelectOption
                    {
                        Order = 4,
                        Text = "Mostre-me um lugar magestoso!"
                    },
                    new SelectOption
                    {
                        Order = 5,
                        Text = "Resuma em uma mensagem só!"
                    },
                    new SelectOption
                    {
                        Order = 6,
                        Text = "Cobre pelo serviço!"
                    },
                }
            };

            await _sender.SendMessageAsync(select, message.From, cancellationToken);
        }
    }
}
