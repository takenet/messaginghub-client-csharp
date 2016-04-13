In most cases, you will not need to handle notifications. But if you do, this document might help.

## Estabelecendo uma conexão

Se você não estiver usando o pacote *Takenet.MessagingHub.Client.Template*, você precisará estabelecer uma conexão antes de enviar ou receber notificações.

Para estabelecer uma conexão, use o `MessagingHubClientBuilder`:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

await client.StartAsync();
```

**Observação: Se você estiver usando o pacote *Takenet.MessagingHub.Client.Template*, a conexão é gerenciada automaticamente. Veja a sessão [hospedagem](http://messaginghub.io/docs/sdks/hosting). para mais detalhes**

## Enviando notificações

Para enviar uma notificação, você pode usar o seguinte método:

```csharp
var notification = new Notification
{
    To = Node.Parse("user"),
    Event = Event.Received
};

await client.SendNotificationAsync(notification);
```

Ou pode usar um destes métodos de extensão para construir suas notificações:

```csharp
await client.SendNotificationAsync(message.ToReceivedNotification());

await client.SendNotificationAsync(message.ToConsumedNotification());

await client.SendNotificationAsync(message.ToFailedNotification());

await client.SendNotificationAsync(message.ToNotification(Event.Received));
```

## Recebendo notificações

Para receber notificações, instancie um listener e registre um *NotificationReceiver*:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

client.AddNotificationReceiver(new ConsumedNotificationReceiver(), Event.Consumed);

await client.StartAsync();
```

**Observação: Se você estiver usando o pacote *Takenet.MessagingHub.Client.Template*, o listener é gerenciado automaticamente. Veja a sessão [hospedagem](http://messaginghub.io/docs/sdks/hosting). para mais detalhes**

Seu *NotificationReceiver* pode ser definido da seguinte forma:

```csharp
public class ConsumedNotificationReceiver : INotificationReceiver
{
    public async Task ReceiveAsync(MessagingHubSender sender, Notification notification, CancellationToken cancellationToken)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}
```

## Notificações de reconhecimento

- Antes de o método `ReceiveAsync` ser executado, uma notificação do tipo `Event.Received` é automaticamente enviada a quem lhe enviou a mensagem.
- Após o método `ReceiveAsync` ser executado, caso nenhuma exceção tenha ocorrido, uma notificação do tipo `Event.Consumed` é automaticamente enviada a quem lhe enviou a mensagem.
- Caso uma exceção tenha ocorrido no método `ReceiveAsync`, uma notificação do tipo `Event.Failed` é automaticamente enviada a quem lhe enviou a mensagem.