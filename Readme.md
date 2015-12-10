**Work is in progress**

Simple [Messaging Hub](http://msging.net/) client with a fluent-style construction.

All you need is:

```c#
var client = new MessagingHubClient("server.msging.net")
                .UsingAccount("myaccount", "mypassword")
                .AddReceiver(new DefaultMessageReceiver());

var executionTask = await client.StartAsync();
```
    
And `DefaultMessageReceiver` class could be as simple as:

```c#
class DefaultMessageReceiver : MessageReceiverBase
{
    public async override Task ReceiveAsync(Message message)
    {
        Trace.WriteLine(message.Content.ToString());
        await Sender.SendMessageAsync("Thanks for you message", message.From);
    }
}
```

To send a message, after starting the client, just call:

```c#
await client.MessageSender.SendMessageAsync("Hello, world", to: "user");
```

Or even simpler (by means of an included extension method):
```c#
await client.SendMessageAsync("Hello, world", to: "user");
```
