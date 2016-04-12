Sending and receiving message is the main purpose of the Messaging Hub Client.

## Establishing a connection

If you are not using the *Takenet.MessagingHub.Client.Template* package, you need to establish a connection before sending or receiving messages.

To establish a connection, use the `MessagingHubConnectionBuilder`:

```csharp
var connection = new MessagingHubConnectionBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

await connection.ConnectAsync();
```

**Remark: If you are using the *Takenet.MessagingHub.Client.Template* package, the connection is automatically handled for you. See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).**

## Sending Messages

To send a message, you can use the following method, or any of its overloads and extensions:

```csharp
await client.SendMessageAsync("text", "destination");
```

To send messages inside a **MessageReceiver**, you should use the **sender** parameter in the *ReceiveAsync* method.

## Receiving Messages

To receive a message, instantiate a listener and register a *MessageReceiver*:

```csharp

var listener = new MessagingHubListener(connection);
listener.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText);

await listener.StartAsync();
```

**Remark: If you are using the *Takenet.MessagingHub.Client.Template* package, the listener is automatically handled for you. See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).**

Your *MessageReceiver* can be defined as follows:

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

And you can respond to the received messages using the *sender* parameter:

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

## Acknoledgement notifications

- Before the `ReceiveAsync` method is executed, a notification of type `Event.Received` is automatically sent to the peer who sent you the message.
- After the `ReceiveAsync` method is executed, if no exception occurs, a notification of type `Event.Consumed` is automatically sent to the peer who sent you the message.
- In case an exception occurs in the `ReceiveAsync` method, a notification of type `Event.Failed` is automatically sent to the peer who sent you the message.