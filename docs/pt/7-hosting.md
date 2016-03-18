## Aplicação console

Uma maneira rápida de começar a desenvolver é utilizando o pacote inicial para um *ConsoleHost* em uma **Console Application** do Visual Studio.

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

Através do arquivo `application.json`, o desenvolvedor tem acesso a todas as propriedades do `MessagingHubClientBuilder`, além de permitir a inicialização de forma transparente dos tipos utilizados pela aplicação. Isso significa que não é necessário se preocupar como a aplicação sera construída para funcionar, já que isso é tratado pelo utilitário `mhh.exe` instalado junto ao pacote. 

Para testar sua aplicação, no Visual Studio, defina o projeto *Class Library* criado como projeto de inicialização. Para isso, na janela **Solution Explorer**, clique com o botão direto no projeto e escolha a opção **Set as StartUp Project**. Depois disso, basta iniciá-la clicando em **Start** ou pressionando F5.


### Application.json

Abaixo, todas as propriedades que podem ser definidas no arquivo `application.json`:

| Propriedade | Descrição                                                                        | Exemplo                 |
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
