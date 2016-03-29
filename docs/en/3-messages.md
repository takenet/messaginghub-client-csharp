The client allow you to send and receive messages through the Messaging Hub.

## Sending Messages

To send a message, you can use the following method:

```csharp
var message = new Message
{
    To = Node.Parse("user"),
    Content = new PlainText()
    {
        Text = "Message Text"
    }
};

await client.SendMessageAsync(message);

```

Or you can use these extension methods to construct and send your message:

```csharp
await client.SendMessageAsync("Message Text", to: "user");

await client.SendMessageAsync("Message Text", Node.Parse("user"));
```

To send messages inside a **receiver**, you should use the **EnvelopeSender** property defined in the base class.

## Receiving Messages

If you are using the Messaging Hub host package, your application receives messages through a **message receiver** registered in the `application.json` file. For more details, please refer to the **Hosting** section.

To receive a message directly from the client, you should call ReceiveMessageAsync method as bellow:

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

using (var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var message = await client.ReceiveMessageAsync(cancellationToken.Token);
}

await client.StopAsync();

```

You can also create a Receiver class that will handle the inbound messages:

```csharp
public class MyMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}

```
And then set it in the builder:

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .AddMessageReceiver(new MyMessageReceiver())
                 .Build();

await client.StartAsync();
```
It is also possible to pass a factory method to construct the receiver:

```csharp
AddMessageReceiver(() => new MyMessageReceiver(), MediaTypes.PlainText);
```

And you can specify a `media type` to filter your messages

```csharp
AddMessageReceiver(() => new MyMessageReceiver(), new MediaType(MediaType.DiscreteTypes.Application, MediaType.SubTypes.JSON));
```
