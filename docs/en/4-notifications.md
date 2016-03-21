The client allow you to send and receive notifications through the Messaging Hub.

## Receiving Notifications

To receive a notification, you can simply build the client and call ReceiveNotificationAsync:

```csharp
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
You can also create a Receiver class that will handle the inbound notifications:

```csharp
public class MyNotificationReceiver : NotificationReceiverBase
{
    public override async Task ReceiveAsync(Notification notification)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}

```
And then set it in the builder:

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .AddNotificationReceiver(new MyNotificationReceiver())
                 .Build();

await client.StartAsync();
```

It is also possible to pass a factory method to construct the receiver:

```csharp
AddNotificationReceiver(() => new MyNotificationReceiver());
```

And you can specify an event type to filter your notifications

```csharp
AddNotificationReceiver(() => new MyNotificationReceiver(), Event.Received);
```

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

Or you can use these extension methods to construct and send your notification:

```csharp
await client.SendNotificationAsync(notification.ToReceivedNotification());

await client.SendNotificationAsync(notification.ToConsumedNotification());

await client.SendNotificationAsync(notification.ToFailedNotification());

await client.SendNotificationAsync(notification.ToNotification(Event.Received));
```
