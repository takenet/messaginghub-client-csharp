## Aplicação console

Uma maneira rápida de começar a desenvolver é utilizando o pacote inicial para um *host* em uma *Console Application* do Visual Studio.

Adicione um novo projeto de um *Console Application* e a partir do *Package Manager Console*, instale-o usando:

    Install-Package Takenet.MessagingHub.Client.ConsoleHost

*Observação*: este pacote tem como *target* o *framework* 4.6.1, então altere o *target framework* do seu projeto.


## Utilizando o Messaging Hub Host

O Messaging Hub oferece o utilitário `mmh.exe` que realiza o *host* de aplicações definidas em um arquivo `application.json`. Este arquivo permite a construção do cliente do Messaging Hub de forma declarativa.

Abaixo um exemplo de um arquivo `application.json`:

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

Neste exemplo, o cliente está sendo configurado utilizando o login `myapplication` e access key `MTIzNDU2`, além de estar registrando um **MessageReceiver** do tipo `PlainTextMessageReceiver`, com um filtro pelo **media type** `text/plain`. A mesma definição utilizando C# seria:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("myapplication", "MTIzNDU2")
    .AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)
    .Build();
```
