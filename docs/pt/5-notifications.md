# Notificações

O cliente permite que você envie e receba notificações através do Messaging Hub.

## Recebendo Notificações

Para receber uma notificação você pode construir o cliente e chamar ReceiveNotificationAsync:

```
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

using(var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var notification = await client.ReceiveNotificationAsync(cancellationToken.Token);
}

await client.StopAsync();

```
Você tambem pode construir um Receiver para tratar as notificações recebidas:

``` 
public class MyNotificationReceiver : NotificationReceiverBase
{
    public override async Task ReceiveAsync(Notification notification)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}

```
E adicionar no builder:

```
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .AddNotificationReceiver(new MyNotificationReceiver())
                 .Build();

await client.StartAsync();
```
Também é possível passar um factory method para construir o receptor:

``` 
AddNotificationReceiver(() => new MyNotificationReceiver());
```

E você pode especificar um `event type` para filtrar suas notificações:

``` 
AddNotificationReceiver(() => new MyNotificationReceiver(), Event.Received);
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
await client.SendNotificationAsync(notification.ToReceivedNotification());

await client.SendNotificationAsync(notification.ToConsumedNotification());

await client.SendNotificationAsync(notification.ToFailedNotification());

await client.SendNotificationAsync(notification.ToNotification(Event.Received));
```