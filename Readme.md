
# Messaging Hub Client

**Work is in progress**

MessagingHub.Client is a simple client for the [Messaging Hub](https://messaginghub.io/) that uses a fluent interface to send and receive messages, commands and notifications.

## Instantiating a client:

```CSharp
const string login = "guest";
const string password = "guest";

var client = new MessagingHubClient() // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
                .UsingAccount(login, password);
```

## Subscribing to receive a plain text message:

```CSharp 
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        Console.WriteLine(message.Content.ToString());
        await MessageSender.SendMessageAsync("Thanks for your message!", message.From);
    }
}

client.AddMessageReceiver(messageReceiver: new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText)
```

## Subscribing to receive a notification:

```CSharp 
public class PrintNotificationReceiver : NotificationReceiverBase
{
    public override Task ReceiveAsync(Notification notification)
    {
        Console.WriteLine("Notification of {0} event received. Reason: {1}", notification.Event, notification.Reason);
        return Task.FromResult(0);
    }
}

client.AddNotificationReceiver(receiverBuilder: () => new PrintNotificationReceiver());
```


## Starting the client:

```CSharp 
// AFTER registered the reveivers, the client MUST be started
await client.StartAsync();

```

## Sending a command and accessing its response:

```CSharp 
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var responseCommand = await client.SendCommandAsync(command);

var account = (Account)responseCommand.Resource;

Console.WriteLine(account.Email);
```

## Publishing a message:

```CSharp 
await client.SendMessageAsync("Hello, world", to: "user");
```

## Disconnecting:

```CSharp 
await client.StopAsync();
```