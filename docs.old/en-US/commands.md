<h1>Commands</h1>

<p>The client allow you to send commands through the Messaging Hub. But it is not possible to receive commands.</p>

<h2>Sending Commands</h2>

<p>To send a command, you can use the following method:</p>

<pre><code>
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var response = await client.SendCommandAsync(command);
</code></pre>

<p>Unlike the messages and notifications, when you send a command, you receive a response when the task completes. This response will contain information about the result of the execution of the command you have sent.</p>

<p><a href="./index.md">Back to the Index</a></p>