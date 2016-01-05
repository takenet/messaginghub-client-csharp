# Home

MessagingHub.Client is a simple client for the [Messaging Hub](https://messaginghub.io/) that uses a fluent interface to send and receive messages, commands and notifications.

# Introduction

The Messaging Hub Client was designed to make it easy to work with the lime protocol client to exchange messages, commands and notifications among the applications and services connected through the Messaging Hub.

The client is available in multiple programming languages, like [C#](https://github.com/takenet/messaginghub-client-csharp), [Java](https://github.com/takenet/messaginghub-client-java) and [Javascript](https://github.com/takenet/messaginghub-client-js), and uses the same semantics in all of them.

The source code is available on [GitHub](https://github.com/takenet) and can be used for reference and also for community contribution. If you want to improve the client, fork the project and send us a pull request.

## Working with the Messaging Hub Client

The following operations are supported:

- Receive messages through the Messaging Hub;

- Send messages through the Messaging Hub;

- Receive notifications through the Messaging Hub;

- Send notifications through the Messaging Hub;

- Send commands through the Messaging Hub;

For receiving envelopes (messages or notifications) the client requests that the developer register receiver agents that will filter the received data and execute the desired action over it, like the example below:

``` 
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Do something with the received message
    }
}

// Register a receiver for messages of the `media type` 'text/plain'
client.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)
```

For sending operations, the client provides Send methods that can be invoked directly, like the example below:

```
// Send a plain text message to the 'user@msging.net' 
await client.SendMessageAsync("Hello, world", to: "user");
```

For more information about specific usage, see the detailed documentation for [Getting Started](http://messaginghub.io/docs/sdks/getting-started), [Client Configuration](http://messaginghub.io/docs/sdks/client-configuration), [Messages](http://messaginghub.io/docs/sdks/messages), [Notifications](http://messaginghub.io/docs/sdks/notifications) and [Commands](http://messaginghub.io/docs/sdks/commands).