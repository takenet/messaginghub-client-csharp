The Messaging Hub Client was designed to make it easy to work with the [Lime protocol](http://limeprotocol.org) client to exchange messages among the applications and services connected through fluent interface.

It is available for the C# programming language in our [GitHub](https://github.com/takenet/messaginghub-client-csharp).

## Preparing the project

To get started, create a new *Client library* using the *Visual Studio 2015 Update 1* and the *.NET Framework 4.6.1*.

From the Package Manager Console, install the application template using:

    Install-Package Takenet.MessagingHub.Client.Host

## Client Configuration

To work with the Messaging Hub you need perform the authentication process.
See more in [Client Configuration](http://messaginghub.io/docs/sdks/clientconfiguration).

## Working with the Messaging Hub

After reference the Messaging Hub.Client in your code and have the data to access, use the following operations:
- Send and receive [messages](http://messaginghub.io/docs/sdks/messages)
- Send and receive [notifications](http://messaginghub.io/docs/sdks/notifications)
- Send [commands](http://messaginghub.io/docs/sdks/commands)

## Hosting

Finally, you can host your code with us so that your work with the Messaging Hub will always be available.
See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).

## Contributions

The source code is available on [GitHub](https://github.com/takenet) and can be used for reference and also for community contribution. If you want to improve the client, fork the project and send us a pull request.
