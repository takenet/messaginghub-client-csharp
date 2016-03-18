## Chave de acesso

Independente da linguagem de programação escolhida, você precisará do nome da aplicação e da chave de acesso.
Para isso, siga os seguintes passos:
- Acesse o [Console](http://messaginghub.io/home/console)
- Clique em [Listar](http://messaginghub.io/application/list) na aba `Aplicações`
- Encontre sua aplicação e clique em `Detalhes`
- Pegue o `Identificador da aplicação` e a `chave de acesso` para utilização

## Configuração

O cliente do Messaging Hub foi projetado para ser simples e fácil de usar, deste modo poucas configurações estão disponíveis.

Para se conectar ao Messaging Hub, o host padrão, `"msging.net"` e o domínio padrão, também `"msging.net"`, são usados por padrão. Caso queira especificar outro host ou domínio, você pode passa-los no builder do cliente, como exibido abaixo:  

```csharp
var client = new MessagingHubClientBuilder()
                 .UsingHostName("iris.0mn")
                 .UsingDomain("0mn")
```

Além do nome do host e do domínio, uma autenticação é obrigatória.
Para autenticar você precisará do identificador e chave de acesso da sua aplicação:

```csharp
const string login = "xpto"; //Identificador da aplicação
const string accessKey = "cXkzT1Rp"; //Chave de acesso da aplicação

var client = new MessagingHubClientBuilder()
                .UsingAccessKey(login, accessKey)
                .Build();
```
O timeout pode ser configurado para requisições enviadas ao servidor:

```csharp
var client = new MessagingHubClientBuilder()
                .UsingAccessKey(login, accessKey)
                .WithSendTimeout(TimeSpan.FromSeconds(20))
                .Build();
```
