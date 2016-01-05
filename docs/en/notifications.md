# Notifications

The client allow you to send and receive notifications through the Messaging Hub.

## Receiving Notifications

To receive a notification, register a receiver like so:

``` 
public class MyNotificationReceiver : NotificationReceiverBase
{
    public override async Task ReceiveAsync(Notification notification)
    {
        // Write the received notification to the console
        Console.WriteLine(notification.ToString());
    }
}

client.AddNotificationReceiver(new MyNotificationReceiver());
```

It is also possible to pass a factory method to construct the receiver:

``` 
client.AddNotificationReceiver(() => new MyNotificationReceiver());
```

And you can specify an event type to filter your notifications

``` 
client.AddNotificationReceiver(() => new MyNotificationReceiver(), Event.Received);
```

## Sending Notifications

To send a notification, you can use the following method:

``` 
var notification = new Notification
{
    To = Node.Parse("user"),
    Event = Event.Received
};

await client.SendNotificationAsync(notification);
```

Or you can use these extension methods to construct and send your notification:

``` 
await client.SendNotificationAsync(message.ToReceivedNotification());

await client.SendNotificationAsync(message.ToConsumedNotification());

await client.SendNotificationAsync(message.ToFailedNotification());

await client.SendNotificationAsync(message.ToNotification(Event.Received));
```