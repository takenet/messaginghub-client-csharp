The Messaging Hub Client was designed to make it easy to work with the [Lime protocol](http://limeprotocol.org) client to exchange messages among the applications and services connected through fluent interface.

It is available for the C# programming language in our [GitHub](https://github.com/takenet/messaginghub-client-csharp).

## Preparing the project

To get started, create a new *Class library* using the *Visual Studio 2015 Update 1* and the *.NET Framework 4.6.1*.

From the Package Manager Console, install the application template using:

    Install-Package Takenet.MessagingHub.Client.Template

## Getting your access key

You will need an application account and access key to interact with the Messaging Hub. To get those, do as follows:
- Access the Messaging Hub [Console](http://messaginghub.io/home/console).
- In the `Applications` tab click [Add](http://messaginghub.io/application/add) to register your application.
- After finishing the registration, get your application `identifier` and `access key` and use them to connect to the Messaging Hub.

## Working with the Messaging Hub Client

You can perform the following operations using the Messaging Hub Client:
- Send and receive [messages](http://messaginghub.io/docs/sdks/messages)
- Send and receive [notifications](http://messaginghub.io/docs/sdks/notifications)
- Send [commands](http://messaginghub.io/docs/sdks/commands)

## Hosting

You can also host your code with us so that your application become always available.
See more in [Hosting](http://messaginghub.io/docs/sdks/hosting).

## Contributions

The source code is available on [GitHub](https://github.com/takenet) and can be used for reference and also for community contribution. If you want to improve the client, fork the project and send us a pull request.
