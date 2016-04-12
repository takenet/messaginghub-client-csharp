Enviar e receber mensagens é o principal propósito do client do Messaging Hub.

## Estabelecendo uma conexão

Se você não estiver usando o pacote *Takenet.MessagingHub.Client.Template*, você precisará estabelecer uma conexão antes de enviar ou receber mensagens.

Para estabelecer uma conexão, use o `MessagingHubConnectionBuilder`:

```csharp
var connection = new MessagingHubConnectionBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

await connection.ConnectAsync();
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

var listener = new MessagingHubListener(connection);
listener.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText);

await listener.StartAsync();
```

**Observação: Se você estiver usando o pacote *Takenet.MessagingHub.Client.Template*, o listener é gerenciado automaticamente. Veja a sessão [hospedagem](http://messaginghub.io/docs/sdks/hosting). para mais detalhes**

Seu *MessageReceiver*  pode ser definido da seguinte forma;

```csharp
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(MessagingHubSender sender, Message message, CancellationToken token)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}
```

Você pode também responder a mensagens recebidas usando o parâmetro *sender*:

```csharp
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(MessagingHubSender sender, Message message, CancellationToken token)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
        // Responds to the received message
        sender.SendMessageAsync("Hi. I just received your message!", message.From, token);
    }
}
```

## Notificações de reconhecimento

- Antes de o método `ReceiveAsync` ser executado, uma notificação do tipo `Event.Received` é automaticamente enviada a quem lhe enviou a mensagem.
- Após o método `ReceiveAsync` ser executado, caso nenhuma exceção tenha ocorrido, uma notificação do tipo `Event.Consumed` é automaticamente enviada a quem lhe enviou a mensagem.
- Caso uma exceção tenha ocorrido no método `ReceiveAsync`, uma notificação do tipo `Event.Failed` é automaticamente enviada a quem lhe enviou a mensagem.