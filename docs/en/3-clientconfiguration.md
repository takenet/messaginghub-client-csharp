
## Access key
You need the application identifier and access key. To do so, follow these steps:
- Access [Console](http://messaginghub.io/home/console)
- Click [List](http://messaginghub.io/application/list) in `Applications` tab
- Find your application and click the `Details` button
- Get the `Application identifier` and `Access Key` to use

## Configuration

The Messaging Hub client is designed to be simple and easy to use, this way few configuration options are available.

To connect to the Messaging Hub, the default host, `"msging.net"` and default domain, also `"msging.net"`, are used by default. 
If you want to specify another host name or domain, you can pass them in the builder of the client, as shown below:

```csharp
var client = new MessagingHubClientBuilder()
                 .UsingHostName("iris.0mn")
                 .UsingDomain("0mn")
```

Besides the host name and domain, an authentication is mandatory. 
The authentication requires identifier and access key of the application:

```csharp
const string login = "xpto"; //Application identifier
const string accessKey = "cXkzT1Rp"; //Access key of the application

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
