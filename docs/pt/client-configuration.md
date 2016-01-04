# Configuração do Cliente

O cliente do Messaging Hub foi projetado para ser simples e fácil de usar, deste modo poucas configurações estão disponíveis.

Para se conectar ao Messaging Hub, o host padrão, `"msging.net"` e o domínio padrão, também `"msging.net"`, são usados por padrão. Caso queira especificar outro host ou domínio, você pode passa-los no construtor do cliente, como exibido abaixo:  

```C#
var client = new MessagingHubClient("meuhost.com", "meudominio.com")
```

Além do nome do host e do domínio, uma autenticação é obrigatória. Essa autenticação pode ser na forma de um login e senha, ou na forma de um login e uma chave de acesso, sendo o método da chave de acesso preferencial.

### Usando login e senha:

```C#
const string login = "user";
const string password = "password";

// Uma vez que o nome do host e a senha não foram informados, os valores padrões serão usados.
var client = new MessagingHubClient()
                .UsingAccount(login, password);
```

### Usando login e chave de acesso:

```C#
const string login = "user";
const string accessKey = "key";

// Uma vez que o nome do host e a senha não foram informados, os valores padrões serão usados.
var client = new MessagingHubClient()
                .UsingAccessKey(login, accessKey);
```