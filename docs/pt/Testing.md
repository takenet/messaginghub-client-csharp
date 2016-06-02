## Utilizando o Messaging Hub Host

O Messaging Hub oferece o utilitário `mhh.exe` que realiza o *host* de aplicações definidas em um arquivo `application.json`. Este arquivo permite a construção do cliente do Messaging Hub de forma declarativa.

Para utilizá-lo, crie um projeto no Visual Studio do tipo **Class library** e instale o pacote com o comando:

    Install-Package Takenet.MessagingHub.Client.Template

Após a instalação, serão adicionados alguns arquivos no projeto, dentre eles o `application.json` com alguns valores padrão definidos. Para a aplicação funcionar, é necessário complementá-lo com algumas informações, como o identificador da sua aplicação (account) e sua chave de acesso (access key).

Abaixo um exemplo:

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

Neste exemplo, o cliente está sendo configurado utilizando a aplicação `xpto` e access key `cXkzT1Rp`, além de estar registrando um **MessageReceiver** do tipo `PlainTextMessageReceiver`, com um filtro pelo **media type** `text/plain`. A mesma definição utilizando C# seria:

```csharp
var client = new MessagingHubClientBuilder()
    .UsingAccessKey("xpto", "cXkzT1Rp")
    .Build();

client.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)

await client.StartAsync();
```

Através do arquivo `application.json`, o desenvolvedor tem acesso a todas as propriedades do `MessagingHubClientBuilder`, além de permitir a inicialização de forma transparente dos tipos utilizados pela aplicação. Isso significa que não é necessário se preocupar como a aplicação sera construída para funcionar, já que isso é tratado pelo utilitário `mhh.exe` instalado junto ao pacote. 

Para testar sua aplicação, no Visual Studio, defina o projeto *Class Library* criado como projeto de inicialização. Para isso, na janela **Solution Explorer**, clique com o botão direto no projeto e escolha a opção **Set as StartUp Project**. Depois disso, basta iniciá-la clicando em **Start** ou pressionando F5.