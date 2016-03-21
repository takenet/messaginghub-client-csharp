Este cliente do Messaging Hub foi desenvolvido para tornar mais fácil o trabalho com o [protocolo Lime](http://limeprotocol.org) para troca de mensanges entre as aplicações e serviços de mensageria integrados ao Messaging Hub através de interface fluente.

Está disponível em algumas linguagens de programação com a mesma semântica, como [C#](https://github.com/takenet/messaginghub-client-csharp) e [Javascript](https://github.com/takenet/messaginghub-client-js).

## Instalação

A partir do *Package Manager Console*, instale o SDK usando:

    Install-Package Takenet.MessagingHub.Client

*Observação*: este pacote tem como *target* o *framework* 4.6.1, então altere o *target framework* do seu projeto.


## Configuração
Para trabalhar com o Messaging Hub você precisará passar pelo processo de autenticação. 
Veja mais detalhes em [Configuração do Cliente](http://messaginghub.io/docs/sdks/clientconfiguration).

## Trabalhando com Messaging Hub

Após referênciar o MessagingHub.Client no seu código e de posse dos dados para acesso, siga os passos descritos em [Como Começar?](http://messaginghub.io/docs/sdks/gettingstarted) para poder utilizar as seguintes operações:
- Enviar e receber [mensagens](http://messaginghub.io/docs/sdks/messages)
- Enviar e receber [notificações](http://messaginghub.io/docs/sdks/notifications)
- Enviar [comandos](http://messaginghub.io/docs/sdks/commands)

## Hospedagem

Por fim, você pode hospedar seu código conosco para que seu trabalho com o Messaging Hub esteja sempre disponível.
Veja mais detalhes em [Hospedagem](http://messaginghub.io/docs/sdks/hosting).

## Contribuições

Os códigos fonte estão disponíveis no [GitHub](https://github.com/takenet) e podem ser usados para referência e também para contribuição da comunidade. Se você deseja melhorar o cliente, fork o projeto e nos envie um pull request.
