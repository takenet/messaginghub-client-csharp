The Messaging Hub Client was designed to make it easy to work with the [Lime protocol](http://limeprotocol.org) client to exchange messages among the applications and services connected through fluent interface.

It is available for the C# programming language in our [GitHub](https://github.com/takenet/messaginghub-client-csharp).

## Preparing the project

To get started, create a new *Class library* using the *Visual Studio 2015 Update 1* and the *.NET Framework 4.6.1*.

From the Package Manager Console, install the application template using:

    Install-Package Takenet.MessagingHub.Client.Template

## Getting your access key

You will need an application identifier and access key to interact with the Messaging Hub. To get those, do as follows:
- Access the [Omni Portal](http://portal.messaginghub.io).
- In the `Contacts` tab go to`Create Contact`.
- Set the required parameters and choose the `API Integration` template.
- Now that your contact has been created, the application identifier and access key are shown in the next step.

## Working with the Messaging Hub Client

You can perform the following operations using the Messaging Hub Client:
- Send and receive [messages](http://portal.messaginghub.io/#/docs/messages)
- Send and receive [notifications](http://portal.messaginghub.io/#/docs/notifications)
- Send [commands](http://portal.messaginghub.io/#/docs/commands)

## Hosting

You can also host your code with us so that your application become always available.
See more in [Hosting](http://portal.messaginghub.io/#/docs/hosting).

## Contributions

The source code is available on [GitHub](https://github.com/takenet) and can be used for reference and also for community contribution. If you want to improve the client, fork the project and send us a pull request.
