## Chave de acesso

Independente da linguagem de programação escolhida, você precisará do identificador da aplicação e chave de acesso.
Seu contato tem que seguir o template `Integração Api` para você ter acesso aos dados.
Siga os seguintes passos:
- Acesse o [Painel Omni](http://portal.messaginghub.io).
- Na aba `Contatos` clique no ícone para editar o seu contato.
- Pronto, os dados serão exibidos na tela.

O identificador e a chave de acesso dever ser definidos no arquivo `application.json`.

## Application.json

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
| serviceProviderType | Um tipo a ser usado como provedor de serviços para injeção de dependências. Deve ser uma implementação de `IServiceProvider`. | ServiceProvider |
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
