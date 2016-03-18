## Using the Console Host

A fast way to build an application connected to the Messaging Hub is to use the package *ConsoleHost*, in a Visual Studio **Console Application** project.

Create a new Console Application project and from the **Package Manager Console**, install the package using the following command:

    Install-Package Takenet.MessagingHub.Client.ConsoleHost

This package will prepare your brand new Console Application with the boilerplate code necessary to connect to the Messaging Hub.

*Note*: this package targets *framework* 4.6.1, so change your project target framework accordingly.

## Using the Messaging Hub Host

The Messaging Hub offers the utility `mmh.exe` which *hosts* applications defined in an `application.json` file. This file allows the creation of Messaging Hub client application in a declarative way.

To use it, create a Visual Studio *Class Library* project and install the package using the folllowing command:

    Install-Package Takenet.MessagingHub.Client.Host

After the installation, some files will be added to your project, among them the `application.json` with some default values defined.
In order to the application to work, it is necessary to complement it with some information, suche as your application login and access key.

Here follows an example:

```json
{
  "login": "myapplication",
  "accessKey": "MTIzNDU2",
  "messageReceivers": [
    {
      "type": "PlainTextMessageReceiver",
      "mediaType": "text/plain"
    }
  ]
}
```

In this example, the client is configured using the application login `myapplication` and the access key `MTIzNDU2`. Besides, it is also registering a **MessageReceiver** of type `PlainTextMessageReceiver`, with a filter of **media type** `text/plain`. The same definition using C# would be:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("myapplication", "MTIzNDU2")
    .AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)
    .Build();
```

Through the `application.json` file, the developer has access to all properties of the `MessagingHubClientBuilder`, also allowing him to initialize, in a transparent maner, the data type used by his application. This means it is not necessary to worry about how the application will be build in order to work, since it is already handled by the `mhh.exe` utility, installed with the package.

To test your application, in Visual Studio, define the *Class Library* project as startup project. To do that, in the **Solution Explorer** window, right click your *Class Library* project and choose the option **Set as StartUp Project**. After that, just click *Start* or press F5.


### Application.json
Here follows all properties defined in the `application.json` file:

| Property    | Description                                                                      | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| login       | The identifier of the Messaging Hub application, registered through the Portal [messaginghub.io](http://messaginghub.io). | myapplication           |
| domain      | **lime** domain to connect. Currently, the only supported value is `msging.net`.| msging.net              |
| hostName    | Address of the server.                                  | msging.net              |
| accessKey   | Access key to authenticate your application, in **base64** format.         | MTIzNDU2                |
| password    | Password to authenticate your application, in **base64** format.                   | MTIzNDU2                |
| sendTimeout | Timeout to send messages, in milliseconds.                              | 30000                   |
| startupType | Name of the .NET type that will be activated when your client is initialized. It must implement the `IStartable` interface. It may be its simple name (if it is found in the same assembly **assembly** as the file `application.json` file) or a fully qualified name with **assembly** name.    | Startup     |
| settings    | General settings for the application, in the key-value format. This value is injected in the instatiated types, such as **receivers** or the **startupType**. To receive values, such types must receive an instance of the type `IDictionary<string, object>` in their constructors. | { "myApiKey": "abcd1234" }   |
| messageReceivers | Array of **message receivers**, that are types specialized in receiving messages. | *See below* |
| notificationReceivers | Array of **notification receivers**, that are types specialized in receiving notifications. | *See below* |

Each **message receiver** can have the following properties:

| Property | Description                                                                        | Example                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Name of the .NET type to receive messages. It must implement the interface `IMessageReceiver`. It can be a simple name (if it is located in the same **assembly** as the file `application.json`) or a fully qualified name with **assembly** name. | PlainTextMessageReceiver |
| settings    | General settings fot the receiver, in the key-value format. Este valor é injected in the instantiated type. To receive values, the implementation must receive an instance of the type `IDictionary<string, object>` in its constructor. | { "mySetting": "xyzabcd" }   |
| mediaType   | Define a filter for the message type that the **receiver** will process. Only messages of the specified type will be delivered to the instantiated receiver. | text/plain |
| content     | Define a regular expression to filter the content of the messages that the **receiver** will process. Only messages that match the expressionn will be delivered to the instantiated receiver. | Olá mundo |
| sender     | Define a regular expression to fillter the origination of the messages that the **receiver** will process. Only messages sent from accounts that match the expressionn will be delivered to the instantiated receiver. | sender@domain.com |

Cada **notification receiver** pode possuir as seguintes propriedades:

| Propriedade | Descrição                                                                        | Exemplo                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Nome do tipo .NET para recebimento de notificações. O mesmo deve implementar a interface `INotificationReceiver`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**. | PlainTextMessageReceiver |
| settings    | Configurações gerais do receiver, no formato chave-valor. Este valor é  injetado na instância criada. Para receber os valores, a implementação deve esperar uma instância do tipo `IDictionary<string, object>` no construtor. | { "mySetting": "xyzabcd" }   |
| eventType   | Define um filtro de tipo de eventos que o **receiver** pode processar. Apenas notificações do evento especificado serão entregues a instância criada. | received |
