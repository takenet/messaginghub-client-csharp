The client also allow you to send commands to another Messaging Hub accounts.

## Sending Commands

To send a command, you can use the following method:

```csharp
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var response = await client.SendCommandAsync(command);
```

Unlike the messages and notifications, when you send a command, you receive a response when the task completes. This response will contain information about the result of the execution of the command you have sent.
