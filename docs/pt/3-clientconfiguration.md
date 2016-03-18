O cliente do Messaging Hub foi projetado para ser simples e fácil de usar, deste modo poucas configurações estão disponíveis.

Para se conectar ao Messaging Hub, o host padrão, `"msging.net"` e o domínio padrão, também `"msging.net"`, são usados por padrão. Caso queira especificar outro host ou domínio, você pode passa-los no builder do cliente, como exibido abaixo:  

```csharp
var client = new MessagingHubClientBuilder()
                 .UsingHostName("iris.0mn")
                 .UsingDomain("0mn")
```

Além do nome do host e do domínio, uma autenticação é obrigatória.
Para autenticar você irá precisar de seu Login e AccessKey do Messaging Hub:

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
