## Testes

Para facilitar o esforço para testar sua aplicação, existe um pacote inicial para um *host* em uma *Console Application*.

Adicione um novo projeto de um *Console Application* e a partir do *Package Manager Console*, instale-o usando:

    Install-Package Takenet.MessagingHub.Client.ConsoleHost

*Observação*: este pacote tem como *target* o *framework* 4.6.1, então altere o *target framework* do seu projeto.

## Produção

Em ambientes de produção uma solução mais robusta, como um *Windows Service*, deve ser utilizada.
