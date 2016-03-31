O cliente do Messaging Hub foi desenvolvido para tornar mais fácil o trabalho com o [protocolo Lime](http://limeprotocol.org) para troca de mensanges entre as aplicações e serviços de mensageria integrados ao Messaging Hub através de interface fluente.

Ele está disponível para a linguagens de programação C# em nosso [GitHub](https://github.com/takenet/messaginghub-client-csharp).

## Preparando o projeto

Para começar, crie uma nova *Class library* usando o *Visual Studio 2015 Update 1* e o *.NET Framework 4.6.1*.

A partir do Package Manager Console, instale o template de aplicações usando:

    Install-Package Takenet.MessagingHub.Client.HostTemplate

## Configurações do cliente

Para trabalhar com o Messaging Hub você precisará passar pelo processo de autenticação. 
Veja mais detalhes em [Configurações do Cliente](http://messaginghub.io/docs/sdks/clientconfiguration).

## Trabalhando com Messaging Hub

Após referênciar o MessagingHub.Client no seu código e de posse dos dados para acesso, poder utilize as seguintes operações:
- Enviar e receber [mensagens](http://messaginghub.io/docs/sdks/messages)
- Enviar e receber [notificações](http://messaginghub.io/docs/sdks/notifications)
- Enviar [comandos](http://messaginghub.io/docs/sdks/commands)

## Hospedagem

Por fim, você pode hospedar seu código conosco para que seu trabalho com o Messaging Hub esteja sempre disponível.
Veja mais detalhes em [Hospedagem](http://messaginghub.io/docs/sdks/hosting).

## Contribuições

Os códigos fonte estão disponíveis no [GitHub](https://github.com/takenet) e podem ser usados para referência e também para contribuição da comunidade. Se você deseja melhorar o cliente, fork o projeto e nos envie um pull request.
