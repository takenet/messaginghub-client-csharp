# Client Configuration

The Messaging Hub client is designed to be simple and easy to use, this way few configuration options are available.

To connect to the Messaging Hub, the default host, `"msging.net"` and default domain, also `"msging.net"`, are used by default. If you want to specify another host name or domain, you can pass them in the constructor of the client, as shown below:

```
var client = new MessagingHubClient("mycustomhost.com", "mycustomdomain.com")
```

Besides the host name and domain, an authentication is mandatory. This authentication can be in a form of a login and password, or in a form of a login and access key, being the access key method the preferred one.

### Using a login and password:

```
const string login = "user";
const string password = "password";

// Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
var client = new MessagingHubClient()
                .UsingAccount(login, password);
```

### Using a login and access key:

```
const string login = "user";
const string accessKey = "key";

// Since host name and domain name are not informed, the default value, 'msging.net', will be used for both parameters
var client = new MessagingHubClient()
                .UsingAccessKey(login, accessKey);
```