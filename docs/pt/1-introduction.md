O cliente do Messaging Hub foi desenvolvido para tornar mais fácil o trabalho com o [protocolo Lime](http://limeprotocol.org) para troca de mensagens entre as aplicações e serviços de mensageria integrados ao Messaging Hub através de interface fluente.

Ele está disponível para a linguagens de programação C# em nosso [GitHub](https://github.com/takenet/messaginghub-client-csharp).

## Preparando o projeto

Para começar, crie uma nova *Class library* usando o *Visual Studio 2015 Update 1* e o *.NET Framework 4.6.1*.

A partir do Package Manager Console, instale o template de aplicações usando:

    Install-Package Takenet.MessagingHub.Client.Template

## Obtendo sua chave de acesso

Você precisará de um identificador e uma chave de acesso para sua aplicação, para poder interagir com o Messaging Hub. Para obtê-los, faça o seguinte:
- Acesse o [Painel Omni](http://portal.messaginghub.io).
- Na aba `Contatos` clique em `Criar Contato`.
- Preencha com os parâmetros requeridos e na próxima etapa escolha o template `Integração API`
- Pronto, seu contato foi criado e o identificador e chave de acesso serão exibidos.

## Trabalhando com Messaging Hub

Você pode realizar as seguintes operações com o client do Messaging Hub:
- Enviar e receber [mensagens](http://portal.messaginghub.io/#/docs/messages)
- Enviar e receber [notificações](http://portal.messaginghub.io/#/docs/notifications)
- Enviar [comandos](http://portal.messaginghub.io/#/docs/commands)

## Hospedagem

Você também pode hospedar seu código conosco para que sua aplicação esteja sempre disponível.
Veja mais detalhes em [Hospedagem](http://messaginghub.io/docs/sdks/hosting).

## Contribuições

Os códigos fonte estão disponíveis no [GitHub](https://github.com/takenet) e podem ser usados para referência e também para contribuição da comunidade. Se você deseja melhorar o cliente, fork o projeto e nos envie um pull request.
