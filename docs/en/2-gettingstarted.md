# Getting Started

This getting started guide will show you how to use the basic functionalities of the Messaging Hub Client 
To run this samples you will need your MessagingHub Login and AccessKey.

## Sending a text message

This will send a message to "otherUser" with the content "Hello world".

```
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

await client.SendMessageAsync("Hello world", to: "otherUser");

await client.StopAsync();

```

## Receiving a message

This code sample will wait for 30 seconds until a message is received.
You can also use a [Receiver](http://messaginghub.io/docs/sdks/messages) to handle inbound messages.

```
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

using(var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var message = await client.ReceiveMessageAsync(cancellationToken.Token);
}

await client.StopAsync();

```

