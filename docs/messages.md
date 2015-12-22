# Messages

The client allow you to send and receive messages through the Messaging Hub.

## Receiving Messages

To receive a message, register a receiver like so:

```CSharp 
public class MyMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}

client.AddMessageReceiver(new MyMessageReceiver(), MediaTypes.PlainText);
```

It is also possible to pass a factory method to construct the receiver:

```CSharp 
client.AddMessageReceiver(() => new MyMessageReceiver(), MediaTypes.PlainText);
```

And you can specify a `media type` to filter your messages

```CSharp 
client.AddMessageReceiver(() => new MyMessageReceiver(), new MediaType(MediaType.DiscreteTypes.Application, MediaType.SubTypes.JSON));
```

## Sending Messages

To send a message, you can use the following method:

```CSharp 
var message = new Message
{
    To = Node.Parse("user"),
    Content = "Message Text"
};

await client.SendMessageAsync(message);
```

Or you can use these extension methods to construct and send your message:

```CSharp 
await client.SendMessageAsync("Message Text", to: "user");

await client.SendMessageAsync("Message Text", Node.Parse("user"));
```

[Back to the Index](./index.md)