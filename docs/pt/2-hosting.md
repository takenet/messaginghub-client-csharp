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


### Application.json

Abaixo, todas as propriedades que podem ser definidas no arquivo `application.json`:

| Propriedade | Descrição                                                                        | Exemplo                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| identifier     | O identificador da aplicação no Messaging Hub, gerado através do portal [messaginghub.io](http://messaginghub.io). | myapplication           |
| domain      | O domínio **lime** para conexão. Atualmente o único valor suportado é `msging.net`.| msging.net              |
| hostName    | O endereço do host para conexão com o servidor.                                  | msging.net              |
| accessKey   | A chave de acesso da aplicação para autenticação, no formato **base64**.         | MTIzNDU2                |
| password    | A senha da aplicação para autenticação, no formato **base64**.                   | MTIzNDU2                |
| sendTimeout | O timeout para envio de mensagens, em milissegundos.                              | 30000                   |
| sessionEncryption | Modo de encriptação a ser usado.                              | None/TLS                   |
| sessionCompression | Modo de compressão a ser usado.                              | None                   |
| startupType | Nome do tipo .NET que deve ser ativado quando o cliente foi inicializado. O mesmo deve implementar a interface `IStartable`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**.    | Startup     |
| serviceProviderType | Um tipo a ser usado como provedor de serviços para injeção de dependências. Deve ser uma implementação de `IServiceContainer`. | ServiceProvider |
| settings    | Configurações gerais da aplicação, no formato chave-valor. Este valor é injetado nos tipos criados, sejam **receivers** ou o **startupType**. Para receber os valores, os tipos devem esperar uma instância do tipo `IDictionary<string, object>` no construtor dos mesmos. | { "myApiKey": "abcd1234" }   |
| settingsType | Nome do tipo .NET que será usado para deserializar as configurações. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**.    | ApplicationSettings     |
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
| settingsType | Nome do tipo .NET que será usado para deserializar as configurações. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**.    | PlainTextMessageReceiverSettings     |

Cada **notification receiver** pode possuir as seguintes propriedades:

| Propriedade | Descrição                                                                        | Exemplo                 |
|-------------|----------------------------------------------------------------------------------|-------------------------|
| type        | Nome do tipo .NET para recebimento de notificações. O mesmo deve implementar a interface `INotificationReceiver`. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**. | NotificationReceiver |
| settings    | Configurações gerais do receiver, no formato chave-valor. Este valor é  injetado na instância criada. Para receber os valores, a implementação deve esperar uma instância do tipo `IDictionary<string, object>` no construtor. | { "mySetting": "xyzabcd" }   |
| eventType   | Define um filtro de tipo de eventos que o **receiver** pode processar. Apenas notificações do evento especificado serão entregues a instância criada. | received |
| settingsType | Nome do tipo .NET que será usado para deserializar as configurações. Pode ser o nome simples do tipo (se estiver na mesma **assembly** do arquivo `application.json`) ou o nome qualificado com **assembly**.    | NotificationReceiverSettings     |

## Publicando sua aplicação

Para ter a sua aplicação carregada, você precisa publicá-la em um servidor que tenha o serviço *Messaging Hub Application Activator* em execução.
Esse serviço irá escanear uma pasta nomeada *MessagingHubApplications* e executar o processo *mhh.exe* para cada subpasta encontrada. Dessa forma, se sua aplicação se chamar *MyApp* e existir uma pasta MyApp na pasta *MessagingHubApplications*, contendo sua *Class library* e seu arquivo *aplication.json*, quando qualquer mudança for detectada no arquivo *application.json*, sua aplicação será recarregada.
Um arquivo nomeado *output.txt* será criado e atualizado para refletir a saída do console da sua aplicação.

### Publicando sua aplicação manualmente

Para publicar sua aplicação manualmente, basta renomear e copiar a pasta *\bin\Release* para a pasta *MessagingHubApplications* de um servidor em que o serviço *Messaging Hub Application Activator* esteja sendo executado.

Para atualizar uma aplicação já publicada, basta sobrescrever a pasta da sua aplicação com os novos arquivos e o serviço irá detectar as mudanças e recarregar a sua aplicação.

### Publicando sua aplicação usando a API

Também é possível publicar sua aplicação nos servidores do *Messaging Hub* usando a *API do Messaging Hub*. Para isso, você precisa fazer um POST contendo o array de bytes que representa a pasta de binários da sua aplicação, zipada, para o endereço http://api.messaginghub.io/Application/{yourappname}/publish. Veja a [documentação da API ](http://api.messaginghub.io/swagger/ui/index#!/Application/Application_PublishAsync) para maiores detalhes.

O arquivo zip deve conter uma única pasta dentro, a qual deve conter todas as *dlls* da sua aplicação e seu arquivo *application.json*. A API irá rejeitar submissões que não passem nesses critérios.

### Publicando sua aplicação usando o Portal

Apesar dos métodos descritos acima, o método **recomendado** para publicar sua aplicação é usar o *Portal do Messaging Hub*. Basta acessar o [Portal](http://messaginghub.io), [listar suas aplicações](http://messaginghub.io/application/list), ir para a página de *detalhes* da aplicação desejada e no painel *Situação*, enviar o arquivo zip contendo sua aplicação.

Once your application is uploaded, it will be detected by the *Messaging Hub Application Activator* service and (re)loaded.
