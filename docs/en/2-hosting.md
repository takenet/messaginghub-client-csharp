## Using the Messaging Hub Host

The Messaging Hub offers the utility `mhh.exe` which *hosts* applications defined in an `application.json` file. This file allows the creation of Messaging Hub client application in a declarative way.

To use it, create a Visual Studio *Class Library* project and install the package using the following command:

    Install-Package Takenet.MessagingHub.Client.Template

After the installation, some files will be added to your project, among them the `application.json` with some default values defined.
In order to the application to work, it is necessary to complement it with some information, such as your application identifier (account) and access key.

Here follows an example:

```json
{
  "identifier": "xpto",
  "accessKey": "cXkzT1Rp",
  "messageReceivers": [
    {
      "type": "PlainTextMessageReceiver",
      "mediaType": "text/plain"
    }
  ]
}
```

In this example, the client is configured using the application `xpto` and the access key `cXkzT1Rp`. Besides, it is also registering a **MessageReceiver** of type `PlainTextMessageReceiver`, with a filter of **media type** `text/plain`. The same definition using C# would be:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

client.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)

await client.StartAsync();
```


Through the `application.json` file, the developer has access to all properties of the `MessagingHubClientBuilder`, also allowing him to initialize, in a transparent maner, the data type used by his application. This means it is not necessary to worry about how the application will be build in order to work, since it is already handled by the `mhh.exe` utility, installed with the package.

To test your application, in Visual Studio, define the *Class Library* project as startup project. To do that, in the **Solution Explorer** window, right click your *Class Library* project and choose the option **Set as StartUp Project**. After that, just click *Start* or press F5.


### Application.json
Here follows all properties defined in the `application.json` file:

| Property    | Description                                                                      | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| identifier     | The identifier of the Messaging Hub application, registered through the Portal [messaginghub.io](http://messaginghub.io). | myapplication           |
| domain      | **lime** domain to connect. Currently, the only supported value is `msging.net`.| msging.net              |
| hostName    | Address of the server.                                  | msging.net              |
| accessKey   | Access key to authenticate your application, in **base64** format.         | MTIzNDU2                |
| password    | Password to authenticate your application, in **base64** format.                   | MTIzNDU2                |
| sendTimeout | Timeout to send messages, in milliseconds.                              | 30000                   |
| sessionEncryption | Encryption mode to be used.                              | None/TLS                   |
| sessionCompression | Encryption mode to be used.                              | None                   |
| startupType | Name of the .NET type that will be activated when your client is initialized. It must implement the `IStartable` interface. It may be its simple name (if it is found in the same assembly **assembly** as the file `application.json` file) or a fully qualified name with **assembly** name.    | Startup     |
| serviceProviderType | A type to be used as a service provider for dependency injection. It must be an implementation of `IServiceProvider`. | ServiceProvider |
| settings    | General settings for the application, in the key-value format. This value is injected in the instantiated types, such as **receivers** or the **startupType**. To receive values, such types must receive an instance of the type `IDictionary<string, object>` in their constructors. | { "myApiKey": "abcd1234" }   |
| settingsType | Name of the .NET type that will be used to deserialize the settings. It may be its simple name (if it is found in the same assembly **assembly** as the file `application.json` file) or a fully qualified name with **assembly** name.    | ApplicationSettings     |
| messageReceivers | Array of **message receivers**, that are types specialized in receiving messages. | *See below* |
| notificationReceivers | Array of **notification receivers**, that are types specialized in receiving notifications. | *See below* |

Each **message receiver** can have the following properties:

| Property | Description                                                                        | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Name of the .NET type to receive messages. It must implement the interface `IMessageReceiver`. It can be a simple name (if it is located in the same **assembly** as the file `application.json`) or a fully qualified name with **assembly** name. | PlainTextMessageReceiver |
| settings    | General settings for the receiver, in the key-value format. Este valor é injected in the instantiated type. To receive values, the implementation must receive an instance of the type `IDictionary<string, object>` in its constructor. | { "mySetting": "xyzabcd" }   |
| mediaType   | Define a filter for the message type that the **receiver** will process. Only messages of the specified type will be delivered to the instantiated receiver. | text/plain |
| content     | Define a regular expression to filter the content of the messages that the **receiver** will process. Only messages that match the expressionn will be delivered to the instantiated receiver. | Hello world |
| sender      | Define a regular expression to filter the origination of the messages that the **receiver** will process. Only messages sent from accounts that match the expression will be delivered to the instantiated receiver. | sender@domain.com |
| settingsType | Name of the .NET type that will be used to deserialize the settings. It may be its simple name (if it is found in the same assembly **assembly** as the file `application.json` file) or a fully qualified name with **assembly** name.    | PlainTextMessageReceiverSettings     |

Each **notification receiver** will have the following properties:

| Property | Description                                                                        | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Name of the .NET to reveice notifications. It must implement the interface `INotificationReceiver`. (if it is located in the same **assembly** as the file `application.json`) or a fully qualified name with **assembly** name. | NotificationReceiver |
| settings    | General settings for the receiver,  in the key-value format. This value is injected in the instantiated type. To receive values, the implementation must receive an instance of the type `IDictionary<string, object>` in its constructor. | { "mySetting": "xyzabcd" }   |
| eventType   | Define a filter for the event type the **receiver** will process. Only notifications of the specified event type will be delivered to the instantiated receiver. | received |
| settingsType | Name of the .NET type that will be used to deserialize the settings. It may be its simple name (if it is found in the same assembly **assembly** as the file `application.json` file) or a fully qualified name with **assembly** name.    | NotificationReceiverSettings     |

## Publishing your application

In order to have your application hosted, you need to publish it in a server running the *Messaging Hub Application Activator* service.
This service will scan a *MessagingHubApplications* folder and execute a *mhh.exe* process for each subfolder found. In this way, if your application is named *MyApp* and a MyApp folder exists in the *MessagingHubApplications* folder, containing your *Class library* and your *application.json* file, when any chances are detected in the *application.json* file, your application will be reloaded.
A file named *output.txt* will be created and updated in that folder to reflect the console output of your application.

### Manually publishing your application

To manually publish you application, just rename and copy your *\bin\Release* folder to the *MessagingHubApplications* folder in a server running the *Messaging Hub Application Activator* service.

To update a published application, just overwrite your application folder with the new files and the service will detect the changes and reload your application.

### Publishing your application using the API

It is also possible to publish your application in the default *Messaging Hub* servers using the *Messaging Hub API*. To do so, you need to POST the byte array that represents your application binaries folder, zipped, to the http://api.messaginghub.io/Application/{yourappname}/publish endpoint. See the [API documentation](http://api.messaginghub.io/swagger/ui/index#!/Application/Application_PublishAsync) for more details.

The zip file must contain a single folder inside, which in turn must contain all your application *dlls* and your *application.json* file. The API will reject uploads that do not match this criteria.

### Publishing your application using the Portal

Despite the methods described above, the **recommended** way to publish your application is to use the *Messaging Hub Portal*. Just access the [Portal](http://messaginghub.io), [list your applications](http://messaginghub.io/application/list), go to the *details* page of the desired application and in the *Status* panel, upload the zip file containing your application.

Once your application is uploaded, it will be detected by the *Messaging Hub Application Activator* service and (re)loaded.
