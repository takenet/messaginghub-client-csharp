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

- Facebook Messenger: Máximo de 320 caractéries

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

...

### Geolocalização (Location)

...

### Processando Pagamentos (Invoice, InvoiceStatus e PaymentReceipt)

... 

### Mensagens Compostas (DocumentCollection e DocumentContainer)

...
