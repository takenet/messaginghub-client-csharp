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
| login       | O login da aplicação no Messaging Hub, gerado através do portal messaginghub.io. | myapplication           |
| domain      | O domínio **lime** para conexão. Atualmente o único valor suportado é `msging.net`.| msging.net              |
| hostName    | O endereço do host para conexão com o servidor.                                  | msging.net              |
| accessKey   | A chave de acesso da aplicação para autenticação, no formato **base64**.         | MTIzNDU2                |
| password    | A senha da aplicação para autenticação, no formato **base64**.                   | MTIzNDU2                |
| sendTimeout | O timeout para envio de mensagens, em milisegundos.                              | 30000                   |
| startupType | Nome do tipo .NET que deve ser ativado quando o cliente foi inicializado. O mesmo deve implementar a interface `IStartable`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**.    | Startup     |
| settings    | Configurações gerais da aplicação, no formato chave-valor. Este valor é  injetado nos tipos criados, sejam **receivers** ou o **startupType**. Para receber os valores, os tipos devem esperar uma instância do tipo `IDictionary<string, object>` no construtor dos mesmos. | { "myApiKey": "abcd1234" }   |
| messageReceivers | Array de **message receivers**, que são tipos especializados para recebimento de mensagens. | *Veja abaixo* |
| notificationReceivers | Array de **notification receivers**, que são tipos especializados para recebimento de notificações. | *Veja abaixo* |

Cada **message receiver** pode possuir as seguintes propriedades:

| Propriedade | Descrição                                                                        | Exemplo                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Nome do tipo .NET para recebimento de mensagens. O mesmo deve implementar a interface `IMessageReceiver`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**. | PlainTextMessageReceiver |
| settings    | Configurações gerais do receiver, no formato chave-valor. Este valor é  injetado na instância criada. Para receber os valores, a implementação deve esperar uma instância do tipo `IDictionary<string, object>` no construtor. | { "mySetting": "xyzabcd" }   |
| mediaType   | Define um filtro de tipo de mensagens que o **receiver** pode processar. Apenas mensagens do tipo especificado serão entregues a instância criada. | text/plain |
| content     | Define uma expressão regular para filtrar os conteúdos de mensagens que o **receiver** pode processar. Apenas mensagens que satisfaçam a expressão serão entregues a instância criada. | Olá mundo |
| sender     | Define uma expressão regular para filtrar os originadores  de mensagens que o **receiver** pode processar. Apenas mensagens que satisfaçam a expressão serão entregues a instância criada. | sender@domain.com |

Cada **notification receiver** pode possuir as seguintes propriedades:

| Propriedade | Descrição                                                                        | Exemplo                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Nome do tipo .NET para recebimento de notificações. O mesmo deve implementar a interface `INotificationReceiver`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**. | PlainTextMessageReceiver |
| settings    | Configurações gerais do receiver, no formato chave-valor. Este valor é  injetado na instância criada. Para receber os valores, a implementação deve esperar uma instância do tipo `IDictionary<string, object>` no construtor. | { "mySetting": "xyzabcd" }   |
| eventType   | Define um filtro de tipo de eventos que o **receiver** pode processar. Apenas notificações do evento especificado serão entregues a instância criada. | received |
