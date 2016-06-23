using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace MessageTypes
{
    public class Option6MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option6MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            // O método de pagamento deve ser informado no portal do Messaging Hub

            var invoice = new Invoice
            {
                Currency = "BLR",
                DueTo = DateTime.Now.AddDays(1),
                Items = new[] { new InvoiceItem { Currency = "BRL", Unit = 1, Description = "Serviços de Teste de Tipos Canônicos", Quantity = 1, Total = 1 } },
                Total = 1
            };

            var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net";
            await _sender.SendMessageAsync(invoice, toPagseguro, cancellationToken);

            // O fluxo continua o Option6Part2MessageReceiver
        }
    }


    public class Option6Part2MessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public Option6Part2MessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var invoiceStatus = message.Content as InvoiceStatus;
            switch (invoiceStatus?.Status)
            {
                case InvoiceStatusStatus.Cancelled:
                    await _sender.SendMessageAsync("Tudo bem, não precisa pagar nada.", message.From, cancellationToken);
                    break;
                case InvoiceStatusStatus.Completed:
                    await _sender.SendMessageAsync("Obrigado pelo seu pagamento, mas como isso é apenas um teste, você pode pedir o ressarcimento do valor pago ao PagSeguro.", message.From, cancellationToken);
                    break;
                case InvoiceStatusStatus.Refunded:
                    await _sender.SendMessageAsync("Pronto. O valor que você me pagou já foi ressarcido pelo PagSeguro!", message.From, cancellationToken);
                    break;
            }
        }
    }
}
