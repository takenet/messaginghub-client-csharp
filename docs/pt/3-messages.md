O cliente permite que você envie e receba mensagens através do Messaging Hub.

## Enviando Mensagens

Para enviar uma mensagem, você pode usar o seguinte método:

```csharp
var message = new Message
{
    To = Node.Parse("user"),
    Content = new PlainText()
    {
        Text = "Message Text"
    }
};

await client.SendMessageAsync(message);
```

Ou você pode usar um destes métodos de extensão para construir e enviar sua mensagem:

```csharp
await client.SendMessageAsync("Texto da Mensagem", to: "user");

await client.SendMessageAsync("Texto da Mensagem", Node.Parse("user"));
```

## Recebendo Mensagens

Caso esteja utilizando o host do Messaging Hub, o recebimento é feito através do registro de um **Message Receiver** no arquivo `application.json`. Para maiores detalhes, consulte a seção **Hospedagem**.

Para receber uma mensagem diretamente através do client, utilize o método **ReceiveMessageAsync**:

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .Build();

await client.StartAsync();

using (var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var message = await client.ReceiveMessageAsync(cancellationToken.Token);
}

await client.StopAsync();

```

Você tambem pode definir um **receiver** para tratar as mensagens recebidas:

```csharp
public class MyMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Write the received message to the console
        Console.WriteLine(message.Content.ToString());
    }
}

```
E adicionar no builder:

```csharp
const string login = "user";
const string accessKey = "myAccessKey";

var client = new MessagingHubClientBuilder()
                 .UsingAccessKey(login, accessKey)
                 .AddMessageReceiver(new MyMessageReceiver())
                 .Build();

await client.StartAsync();
```

Também é possível passar um factory method para construir o receptor:

```csharp
AddMessageReceiver(() => new MyMessageReceiver(), MediaTypes.PlainText);
```

E você pode especificar um media type para filtrar suas mensagens:

```csharp
AddMessageReceiver(() => new MyMessageReceiver(), new MediaType(MediaType.DiscreteTypes.Application, MediaType.SubTypes.JSON));
```
