Enviar e receber mensagens é o principal propósito do client do Messaging Hub.

## Estabelecendo uma conexão

Se você não estiver usando o pacote *Takenet.MessagingHub.Client.Template*, você precisará estabelecer uma conexão antes de enviar ou receber mensagens.

Para estabelecer uma conexão, use o `MessagingHubClientBuilder`:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

await client.StartAsync();
```

**Observação: Se você estiver usando o pacote *Takenet.MessagingHub.Client.Template*, a conexão é gerenciada automaticamente. Veja a sessão [hospedagem](http://messaginghub.io/docs/sdks/hosting). para mais detalhes**

## Enviando mensagens

Para enviar uma mensagem, você pode usar o método abaixo, ou uma de suas sobrecargas e extensões:

```csharp
await client.SendMessageAsync("text", "destination");
```

Para enviar mensagens de dentro de um **MessageReceiver**, use o parâmetro **sender** passado ao método *ReceiveAsync*.

## Recebendo mensagens

Para receber uma mensagem, instancie um listener e registre um *MessageReceiver*:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

client.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText);

await client.StartAsync();
```

**Observação: Se você estiver usando o pacote *Takenet.MessagingHub.Client.Template*, o listener é gerenciado automaticamente. Veja a sessão [hospedagem](http://portal.messaginghub.io/#/docs/hosting). para mais detalhes**

Seu *MessageReceiver*  pode ser definido da seguinte forma;

```csharp
public class PlainTextMessageReceiver : IMessageReceiver
{
    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}
```

Você pode também responder a mensagens recebidas usando um *IMessagingHubSender* que você pode injetar no construtor:

```csharp
public class PlainTextMessageReceiver : IMessageReceiver
{
    private readonly IMessagingHubSender _sender;

    public PlainTextMessageReceiver(IMessagingHubSender sender)
    {
        _sender = sender;
    }

    public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
        // Responds to the received message
        _sender.SendMessageAsync("Hi. I just received your message!", message.From, cancellationToken);
    }
}
```

## Notificações de reconhecimento

- Antes de o método `ReceiveAsync` ser executado, uma notificação do tipo `Event.Received` é automaticamente enviada a quem lhe enviou a mensagem.
- Após o método `ReceiveAsync` ser executado, caso nenhuma exceção tenha ocorrido, uma notificação do tipo `Event.Consumed` é automaticamente enviada a quem lhe enviou a mensagem.
- Caso uma exceção tenha ocorrido no método `ReceiveAsync`, uma notificação do tipo `Event.Failed` é automaticamente enviada a quem lhe enviou a mensagem.