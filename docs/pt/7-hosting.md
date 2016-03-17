## Aplicação console

Uma maneira rápida de começar a desenvolver é utilizando o pacote inicial para um *host* em uma **Console Application** do Visual Studio.

Adicione um novo projeto de um **Console Application** e a partir do **Package Manager Console**, instale-o usando:

    Install-Package Takenet.MessagingHub.Client.ConsoleHost

*Observação*: este pacote tem como *target* o *framework* 4.6.1, então altere o *target framework* do seu projeto.

## Utilizando o Messaging Hub Host

O Messaging Hub oferece o utilitário `mmh.exe` que realiza o *host* de aplicações definidas em um arquivo `application.json`. Este arquivo permite a construção do cliente do Messaging Hub de forma declarativa.

Para utilizá-lo, crie um projeto no Visual Studio do tipo **Class library** e instale o pacote com o comando:

    Install-Package Takenet.MessagingHub.Client.Host

Após a instalação, serão adicionados alguns arquivos no projeto, dentre eles o `application.json` com alguns valores padrão definidos. Para a aplicação funcionar, é necessário complementá-lo com algumas informações, como o login e access key.

Abaixo um exemplo:

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

Pelo arquivo `application.json`, o desenvolvedor tem acesso a todas as propriedades do `MessagingHubClientBuilder`, além de permitir a inicialização de forma transparente dos tipos utilizados pela aplicação. Isso significa que não é necessário se preocupar como a aplicação sera construída para funcionar, já que isso é tratado pelo utilitário `mhh.exe` instalado junto ao pacote. 

Para testar sua aplicação, no Visual Studio, defina o projeto *Class Library* criado como projeto de inicialização. Para isso, na janela **Solution Explorer**, clique com o botão direto no projeto e escolha a opção **Set as StartUp Project**. Depois disso, basta iniciá-la clicando em **Start** ou pressionando F5.


