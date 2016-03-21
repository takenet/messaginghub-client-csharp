O cliente permite que você envie comandos através do Messaging Hub. No entanto não é possível receber comandos. 

## Enviando comandos

Para enviar um comando, você pode usar o seguinte método:

```csharp
var command = new Command {
    Method = CommandMethod.Get,
    Uri = new LimeUri("/account")
};

var response = await client.SendCommandAsync(command);
```

Diferentemente de mensagens e notificações, quando você envia um comando, você recebe uma resposta quando a tarefa é concluída. Essa resposta contém informações a respeito do resultado da execução do comando enviado.
