# Como começar?

Este guia de como começar irá apresentar a você o uso das funcionalidades básicas do cliente do Messaging Hub 

## Instanciando o cliente

```CSharp
const string login = "guest";
const string password = "guest";

var client = new MessagingHubClient() // Uma vez que o nome do host e do domínio não foram informados, o valor padrão, 'msging.net', será utilizado para ambos os parâmetros
                .UsingAccount(login, password);
```

## Se inscrevendo para receber uma mensagem de texto

```CSharp 
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        Console.WriteLine(message.Content.ToString());
        await MessageSender.SendMessageAsync("Obrigado por sua mensagem!", message.From);
    }
}

client.AddMessageReceiver(messageReceiver: new PlainTextMessageReceiver(), forMimeType: MediaTypes.PlainText);
```

## Se inscrevendo para receber uma notificação

```CSharp 
public class PrintNotificationReceiver : NotificationReceiverBase
{
    public override Task ReceiveAsync(Notification notification)
    {
        Console.WriteLine("Notificação do evento {0} recebida. Motivo: {1}", notification.Event, notification.Reason);
        return Task.FromResult(0);
    }
}

client.AddNotificationReceiver(receiverBuilder: () => new PrintNotificationReceiver());
```


## Iniciando o cliente

```CSharp 
// APÓS registrados os receptores, o cliente DEVE ser iniciado
await client.StartAsync();
```

## Enviando um comando e acessando sua resposta

```CSharp 
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var responseCommand = await client.SendCommandAsync(command);

var account = (Account)responseCommand.Resource;

Console.WriteLine(account.Email);
```

## Publicando uma mensagem

```CSharp 
await client.SendMessageAsync("Olá, mundo", to: "user");
```

## Desconectando

```CSharp 
await client.StopAsync();
```

[Retornar ao Índice](./index.md)