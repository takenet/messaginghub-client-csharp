## Infraestrutura de Mensagens do Messaging Hub

O Messaging Hub possui uma infraestrutura que permite que os Chat Bots sejam construídos usando uma linguagem canônica, que é devidamente traduzida para as mensagens específicas de cada um dos canais disponíveis, como Facebook Messenger, Skype, SMS.

## Os tipos canônicos disponíveis são:

- **PlainText:**
  Este é o tipo de mensagem padrão e é utilizado para o envio de mensagens de texto simples.
- **MediaLink:**
  O tipo MediaLink é usado para enviar imagens, sons, vídeos e outros arquivos de mídia. Em canais que não suportam essas mídias, um link para um endereço web contendo o arquivo será enviado.
- **WebLink:**
  Um WebLink pode ser usado para enviar links para paginas web. Alguns canais, como o OMNI e o Facebook, fazem um excelente tratamento desse tipo, exibindo uma miniatura da página dentro da própria thread de mensagens.
- **Select:**
  Um Select permite o envio ao cliente do Chat Bot de uma lista de opções, da qual ele pode selecionar uma delas como resposta.
- **Location:**
  O tipo Location pode ser usado pelo canal para enviar ao Chat Bot a localização geográfica do cliente, ou para que o Chat Bot envie ao cliente uma determinada localização.
- **Invoice:**
  O tipo Invoice pode ser usado pelo Chat Bot para solicitar um pagamento a um canal de pagamento, como por exemplo o PagSeguro.
- **InvoiceStatus:**
  InvoiceStatus são mensagens recebidas pelo Chat Bot, a partir do canal de pagamento, comunicando o status do pagamento solicitado.
- **PaymentReceipt:**
  Um PaymentReceipt é o tipo de mensagem que deve ser enviado ao cliente que realizou um pagamento.
- **DocumentCollection:**
  DocumentCollections permitem que múltiplas mensagens sejam enviadas dentro de uma única mensagem.
- **DocumentContainer:**
  Encapsula um conteúdo de forma a ser utilizado junto ao DocumentCollection para envio de multiplos conteúdos diferentes. Util para mandar conteúdos compostos (texto + imagem).

## Exemplos:

Confira o Chat Bot [Message Types](https://github.com/takenet/messaginghub-client-csharp/tree/master/src/Samples/MessageTypes) para ver como usar cada um dois tipos canônicos do Messaging Hub.

### Mensagens de Texto Simples (PlainText)

Mensagens de texto simples são suportadas em todos os canais, no entanto restrições podem se aplicar a alguns deles, como por exemplo o tamanho da mensagem.

*Exemplo:*

O exemplo abaixo mostra como responder a uma mensagem recém recebida com uma mensagem de texto simples.
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new PlainText {Text = "... Inspiração, e um pouco de café! E isso me basta!"};
    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
*Restrições:*

- Facebook Messenger: Máximo de 320 caractéries. Caso seu Chat Bot envie mensagens maiores que este limite, e esteja utilizando este canal, sua mensagem não será enviada.

### Links para Arquivos de Mídia e Páginas Web (MediaLink e WebLink)

Para enviar arquivos de mídia, o documento enviado deve ser do tipo MediaLink, conforme abaixo:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var imageUri = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/4/45/A_small_cup_of_coffee.JPG/200px-A_small_cup_of_coffee.JPG", UriKind.Absolute);

    var document = new MediaLink
    {
        Text = "Café, o que mais seria?",
        Size = 6679,
        Type = MediaType.Parse("image/jpeg"),
        PreviewUri = imageUri,
        Uri = imageUri
    };
    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
Já para enviar o link para um página web, use o tipo WebLink:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var url = new Uri("https://pt.wikipedia.org/wiki/Caf%C3%A9");
    var previewUri =
        new Uri(
            "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/200px-Roasted_coffee_beans.jpg");

    var document = new WebLink
    {
        Text = "Café, a bebida sagrada!",
        PreviewUri = previewUri,
        Uri = url
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
### Enviando listas de opções (Select)

Para enviar uma lista de opções para que o cliente escolha um delas como resposta, utilize o tipo Select:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Select
    {
        Text = "Escolha uma opção:",
        Options = new []
        {
            new SelectOption
            {
                Order = 1,
                Text = "Um texto inspirador!",
                Value = new PlainText { Text = "1" }
            },
            new SelectOption
            {
                Order = 2,
                Text = "Uma imagem motivacional!",
                Value = new PlainText { Text = "2" }
            },
            new SelectOption
            {
                Order = 3,
                Text = "Um link para algo interessante!",
                Value = new PlainText { Text = "3" }
            }
        }
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```
*Observações:*
- A propriedade Value é opcional, Mas caso informada, seu valor seráenviado como resposta quando a opção for escolhida.
- Caso a propriedade Value não seja informada, ou a propriedade Order ou a propriedade Text deve estar presente. Se apenas uma delas estiver presente, este será o valor enviado como resposta. Caso contrário, o valor da propriedade Order será usado.

*Restrições:*
- Facebook Messenger: Limite de 3 opções. Caso precise de mais opções e esteja usando este canal, envie multiplas mensagens, cada uma com no máximo 3 opções, caso contrário a mensagem não será enviada.
- Tangram SMS: A propriedade valor será ignorada e o valor da propriedade Order deverá ser enviado como resposta indicando a opção selecionada.

### Geolocalização (Location)

Um Chat Bot pode enviar uma localização ao cliente ou o canal pode enviar ao Chat Bot a localização do cliente. Em ambos os casos, o tipo Location será usado:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Location
    {
        Latitude = -22.121944,
        Longitude = -45.128889,
        Altitude = 1143
    };

    await _sender.SendMessageAsync(document, message.From, cancellationToken);
}
```

*Restrições:*
- Este tipo não é suportado em nenhum dos canais no momento!

### Processando Pagamentos (Invoice, InvoiceStatus e PaymentReceipt)

Para realizar um pagamento através do seu Chat Bot, é necessário envolver um canal de pagamento. No momento apenas o canal PagSeguro é suportado e para solicitar o pagamento, o Chat Bot deve enviar uma mensagem do tipo Invoice para o canal de pagamento informando o endereço no formato abaixo:

```csharp
var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net"; // Ex: 988887777%400mni.io@pagseguro.gw.msging.net
```

Abaixo um exemplo completo de envio de solicitação de pagamento:
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    var document = new Invoice
    {
        Currency = "BLR",
        DueTo = DateTime.Now.AddDays(1),
        Items =
            new[]
            {
                new InvoiceItem
                {
                    Currency = "BRL",
                    Unit = 1,
                    Description = "Serviços de Teste de Tipos Canônicos",
                    Quantity = 1,
                    Total = 1
                }
            },
        Total = 1
    };

    var toPagseguro = $"{Uri.EscapeDataString(message.From.ToIdentity().ToString())}@pagseguro.gw.msging.net";
    
    await _sender.SendMessageAsync(document, toPagseguro, cancellationToken);
}
```

*Importante:* Para que esta solicitação de pagamento seja processada, o canal de pagamento PagSeguro deve ser habilitado para a seu Chat Bot no portal do Messaging Hub.

Ao receber esta mensagem, o PagSeguro enviará ao cliente um link para realização do pagamento. Uma vez realizado, ou cancelado, o pagamento, uma mensagem do tipo InvoiceStatus será recebida pelo seu Chat Bot. Para isso um *Receiver* para o MediaType `application/vnd.lime.invoice-status+json`, o qual deve ser registrado no arquivo `application.json` da seguinte forma:
```json
  "messageReceivers": [
  {
    {
      "type": "InvoiceStatusReceiver",
      "mediaType": "application/vnd.lime.invoice-status\\+json"
    }
}
```
Tal receiver deve ser definido da seguinte forma:
```csharp
public class InvoiceStatusReceiver : IMessageReceiver
{
    private readonly IMessagingHubSender _sender;

    public InvoiceStatusReceiver(IMessagingHubSender sender)
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
                await _sender.SendMessageAsync("Obrigado pelo seu pagamento, mas como isso é apenas um teste, você pode pedir o ressarcimento do valor pago ao PagSeguro. Em todo caso, segue o seu recibo:", message.From, cancellationToken);
                var paymentReceipt = new PaymentReceipt
                {
                    Currency = "BLR",
                    Items =
                        new[]
                        {
                            new InvoiceItem
                            {
                                Currency = "BRL",
                                Unit = 1,
                                Description = "Serviços de Teste de Tipos Canônicos",
                                Quantity = 1,
                                Total = 1
                            }
                        },
                    Total = 1
                };
                await _sender.SendMessageAsync(paymentReceipt, message.From, cancellationToken);
                break;
            case InvoiceStatusStatus.Refunded:
                await _sender.SendMessageAsync("Pronto. O valor que você me pagou já foi ressarcido pelo PagSeguro!", message.From, cancellationToken);
                break;
        }
    }
}
```
Como pode ser visto no exemplo acima, seu Chat Bot deve estar preparado para reagir ao 3 statuses disponíveis como resposta ao seu pedido de pagamento, e deve enviar um recibo de pagamento (tipo PaymentReceipt) como resposta ao cliente.

### Mensagens Compostas (DocumentCollection e DocumentContainer)

...
