In most cases, you will not need to handle notifications. But if you do, this document might help.

## Establishing a connection

If you are not using the *Takenet.MessagingHub.Client.Template* package, you need to establish a connection before sending or receiving notifications.

To establish a connection, use the `MessagingHubConnectionBuilder`:

```csharp
var connection = new MessagingHubConnectionBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

await connection.ConnectAsync();
```

**Remark: If you are using the *Takenet.MessagingHub.Client.Template* package, the connection is automatically handled for you. See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).**

## Sending Notifications

To send a notification, you can use the following method:

```csharp
var notification = new Notification
{
    To = Node.Parse("user"),
    Event = Event.Received
};

await client.SendNotificationAsync(notification);
```

Or you can use these extension methods to construct and send your notifications:

```csharp
await client.SendNotificationAsync(message.ToReceivedNotification());

await client.SendNotificationAsync(message.ToConsumedNotification());

await client.SendNotificationAsync(message.ToFailedNotification());

await client.SendNotificationAsync(message.ToNotification(Event.Received));
```

## Receiving Notifications

To receive a notification, instantiate a listener and register a *NotificationReceiver*:

```csharp

var listener = new MessagingHubListener(connection);
listener.AddNotificationReceiver(new ConsumedNotificationReceiver(), Event.Consumed);

await listener.StartAsync();
```

**Remark: If you are using the *Takenet.MessagingHub.Client.Template* package, the listener is automatically handled for you. See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).**

Your *NotificationReceiver* can be defined as follows:

```csharp
public class ConsumedNotificationReceiver : NotificationReceiverBase
{
    public override async Task ReceiveAsync(MessagingHubSender sender, Notification notification, CancellationToken token)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}
```

## Acknoledgement notifications

- Before the `ReceiveAsync` method is executed, a notification of type `Event.Received` is automatically sent to the peer who sent you the message.
- After the `ReceiveAsync` method is executed, if no exception occurs, a notification of type `Event.Consumed` is automatically sent to the peer who sent you the message.
- In case an exception occurs in the `ReceiveAsync` method, a notification of type `Event.Failed` is automatically sent to the peer who sent you the message.