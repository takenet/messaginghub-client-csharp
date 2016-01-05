# Notificações

O cliente permite que você envie e receba notificações através do Messaging Hub.

## Recebendo Notificações

Para receber uma notificação, registre um receptor da seguinte forma:

``` 
public class MyNotificationReceiver : NotificationReceiverBase
{
    public override async Task ReceiveAsync(Notification notification)
    {
        // Escreve a notificação recebida no console
        Console.WriteLine(notification.ToString());
    }
}

client.AddNotificationReceiver(new MyNotificationReceiver());
```

Também é possível passar um factory method para construir o receptor:

``` 
client.AddNotificationReceiver(() => new MyNotificationReceiver());
```

E você pode especificar um `event type` para filtrar suas mensagens

``` 
client.AddNotificationReceiver(() => new MyNotificationReceiver(), Event.Received);
```

## Enviando Notificações

Para enviar uma notificação, você pode usar o seguinte método:

``` 
var notification = new Notification
{
    To = Node.Parse("user"),
    Event = Event.Received
};

await client.SendNotificationAsync(notification);
```

Ou você pode usar um destes métodos de extensão para construir e enviar sua notificação:

``` 
await client.SendNotificationAsync(message.ToReceivedNotification());

await client.SendNotificationAsync(message.ToConsumedNotification());

await client.SendNotificationAsync(message.ToFailedNotification());

await client.SendNotificationAsync(message.ToNotification(Event.Received));
```