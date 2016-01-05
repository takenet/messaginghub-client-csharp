# Mensagens

O cliente permite que você envie e receba mensagens através do Messaging Hub.

## Recebendo Mensagens

Para receber uma mensagem, registre um receptor da seguinte forma:

``` 
public class MyMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Escreve o conteúdo da mensagem no console
        Console.WriteLine(message.Content.ToString());
    }
}

client.AddMessageReceiver(new MyMessageReceiver(), MediaTypes.PlainText);
```

Também é possível passar um factory method para construir o receptor:

``` 
client.AddMessageReceiver(() => new MyMessageReceiver(), MediaTypes.PlainText);
```

E você pode especificar um media type para filtrar suas mensagens

``` 
client.AddMessageReceiver(() => new MyMessageReceiver(), new MediaType(MediaType.DiscreteTypes.Application, MediaType.SubTypes.JSON));
```

## Enviando Mensagens

Para enviar uma mensagem, você pode usar o seguinte método:

``` 
var message = new Message
{
    To = Node.Parse("user"),
    Content = "Texto da Mensagem"
};

await client.SendMessageAsync(message);
```

Ou você pode usar um destes métodos de extensão para construir e enviar sua mensagem:

``` 
await client.SendMessageAsync("Texto da Mensagem", to: "user");

await client.SendMessageAsync("Texto da Mensagem", Node.Parse("user"));
```