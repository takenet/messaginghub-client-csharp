<h1>Messages</h1>

<p>The client allow you to send and receive messages through the Messaging Hub.</p>

<h2>Receiving Messages</h2>

<p>To receive a message, register a receiver like so:</p>

<pre><code>
public class MyMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}

client.AddMessageReceiver(new MyMessageReceiver(), MediaTypes.PlainText);
</code></pre>

<p>It is also possible to pass a factory method to construct the receiver:</p>

<pre><code>
client.AddMessageReceiver(() => new MyMessageReceiver(), MediaTypes.PlainText);
</code></pre>

<p>And you can specify a <code>media type</code> to filter your messages</p>

<pre><code>
client.AddMessageReceiver(() => new MyMessageReceiver(), new MediaType(MediaType.DiscreteTypes.Application, MediaType.SubTypes.JSON));
</code></pre>

<h2>Sending Messages</h2>

<p>To send a message, you can use the following method:</p>

<pre><code>
var message = new Message
{
    To = Node.Parse("user"),
    Content = "Message Text"
};

await client.SendMessageAsync(message);
</code></pre>

<p>Or you can use these extension methods to construct and send your message:</p>

<pre><code>
await client.SendMessageAsync("Message Text", to: "user");

await client.SendMessageAsync("Message Text", Node.Parse("user"));
</code></pre>

<p><a href="./index.md">Back to the Index</a></p>