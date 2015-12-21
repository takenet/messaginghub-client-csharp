# Client Configuration

The Messaging Hub client is designed to be simple and easy to use, this way few configuration options are available.

To connect to the Messaging Hub, the default host, `"msging.net"` and default domain, also `"msging.net"`, are used by default. If you want to specify another host name or domain, you can pass them in the constructor of the client, as shown below:

```CSharp
var client = new MessagingHubClient("mycustomhost.com", "mycustomdomain.com")
```

Despite the host name and domain, an authentication is mandatory. This authentication can be in a form of a login and password, of in a form of a login and access key, being the access key method the preferred one.

Using a login and password:
```CSharp
const string login = "user";
const string password = "password";

var client = new MessagingHubClient() // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
                .UsingAccount(login, password);
```

Using a login and access key:
```CSharp
const string login = "user";
const string accessKey = "key";

var client = new MessagingHubClient() // Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
                .UsingAccessKey(login, accessKey);
```

[Back to the Index](./index.md)