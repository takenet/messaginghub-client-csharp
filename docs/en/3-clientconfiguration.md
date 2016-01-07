# Client Configuration

The Messaging Hub client is designed to be simple and easy to use, this way few configuration options are available.

To connect to the Messaging Hub, the default host, `"msging.net"` and default domain, also `"msging.net"`, are used by default. 
If you want to specify another host name or domain, you can pass them in the builder of the client, as shown below:

```csharp
var client = new MessagingHubClientBuilder()
                 .UsingHostName("iris.0mn")
                 .UsingDomain("0mn")
```

Besides the host name and domain, an authentication is mandatory. 
The authentication requires your login and access key:

```csharp
const string login = "user";
const string accessKey = "accessKey";

var client = new MessagingHubClientBuilder()
                .UsingAccessKey(login, accessKey)
                .Build();
```

You can also set the timeout to requests made to Messaging Hub server:

```csharp
var client = new MessagingHubClientBuilder()
                .UsingAccessKey(login, accessKey)
                .WithSendTimeout(TimeSpan.FromSeconds(20))
                .Build();
```