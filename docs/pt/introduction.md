# Início

MessagingHub.Client é um cliente simples para o [Messaging Hub](https://messaginghub.io/) que usa uma interface fluente para enviar e receber mensagens, comandos e notificações.

# Introdução

O cliente do Messaging Hub foi desenvolvido para tornar mais fácil o trabalho com o cliente do protocolo lime para troca de mensanges, comandos e notificações entre as aplicações e serviços conectadas pelo Messaging Hub.

O cliente está disponível em múltiplas linguagens de programações, como [C#](https://github.com/takenet/messaginghub-client-csharp), [Java](https://github.com/takenet/messaginghub-client-java) and [Javascript](https://github.com/takenet/messaginghub-client-js), e usa a mesma semântica em todas elas.

Os códigos fonte estão disponíveis no [GitHub](https://github.com/takenet) e podem ser usados para referência e também para contribuição da comunidade. Se você deseja melhorar o cliente, fork o projeto e nos envie um pool request.

## Trabalhando com o cliente do Messaging Hub

As seguintes operações são suportados:

- Receber mensagens através do Messaging Hub;

- Enviar  mensagens através do Messaging Hub;

- Receber notificações através do Messaging Hub;

- Enviar notificações através do Messaging Hub;

- Enviar comandos através do Messaging Hub;

Para reveber envelopes (mensagens e notificações) o cliente requer que o desenvolvedor registre agentes receptores que irão filtrar os dados recebidos e executar a ação desejada sobre eles, como no exemplo a seguir:

```C# 
public class PlainTextMessageReceiver : MessageReceiverBase
{
    public override async Task ReceiveAsync(Message message)
    {
        // Faz algo com a mensagem recebida
    }
}

// Registra um receptor para receber mensagens com `media type` 'text/plain'
client.AddMessageReceiver(new PlainTextMessageReceiver(), MediaTypes.PlainText)
```

Para operações de envio, o cliente provê métodos Send que podem ser invocados diretamente, como no exemplo a seguir:

```C#
// Envia uma mensagem de texto para 'user@msging.net' 
await client.SendMessageAsync("Olá, mundo", to: "user");
```

Para mais informações específicas de uso, veja a documentação detalheada para [Como Começar?](../{{page.lang}}/getting-started), [Configuração do Cliente](../{{page.lang}}/client-configuration), [Mensagens](../{{page.lang}}/messages), [Notificações](../{{page.lang}}/notifications) e [Comandos](../{{page.lang}}/commands).