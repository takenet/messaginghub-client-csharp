# Como começar

Este guia de como começar irá apresentar a você o uso das funcionalidades básicas do cliente do Messaging Hub.
Para executar os exemplos, você vai precisar do seu Login e de sua AccessKey.

## Enviando uma mensagem

Uma mensagem é disparada para o usuário "otherUser" com o conteúdo "Hello World".

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

await client.SendMessageAsync("Hello world", to: "otherUser");

await client.StopAsync();

```

## Recebendo uma mensagem

Este exemplo espera por 30 segundos até uma mensagem ser recebida.
Você pode usar também um [Receiver](http://messaginghub.io/docs/sdks/messages) para tratar as mensagens que chegarem.

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

using(var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var message = await client.ReceiveMessageAsync(cancellationToken.Token);
}

await client.StopAsync();

```

