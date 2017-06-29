## Introdução

Neste tutorial vamos usar a API do BING (https://www.microsoft.com/cognitive-services/en-us/bing-news-search-api) para criar um contato que busca imagens de acordo com o texto enviado.

## Passo 1 - Criando o projeto
O primeiro passo é, no Visual Studio, criar um novo projeto do tipo Class Library usando o .NET Framework 4.6.1 e instalar o pacote do SDK via NuGet, através do comando:
Install-Package Takenet.MessagingHub.Client.Template

Desta forma, é adicionado ao projeto entre outras dependências, o arquivo application.json, onde ficam registrados os receivers de mensagens e notificações. Os receivers são as entidades responsáveis por processar as mensagens e notificações recebidas realizando ações específicas (invocando APIs, salvando informações no banco de dados, etc.) e, se necessário, enviar uma resposta ao usuário.
Para mais informações acessar a documentação: https://portal.blip.ai/#/docs/home

## Passo 2 - Configurando o BOT
Vamos obter um identifier e accessKey para configurar o seu application.json, acesse o portal http://blip.ai e registre o seu contato, utilizando a opção Chat Bot SDK. Após a criação do seu contato você vai encontrar as informações no menu configuração, coloque então os valores no seu arquivo application.json.
Com isso já temos um BOT conectado a plataforma que consegue enviar e receber mensagens. Para habilitar o seu contato nos canais, basta acessar o menu PUBLICAÇÔES e escolher o canal onde deseja que seu contato esteja presente (no próprio site do Blip.ai a um guia de como ativar os canais).

## Passo 3 - Ativando Bing News Search API
Agora precisamos ativar a sua conta da Microsoft para usar o serviço Bing News Search API, isso pode ser feito na url https://www.microsoft.com/cognitive-services/en-us/bing-news-search-api. Clique em Letsgo e depois selecione Bing Search após aceitar os termos voce terá acesso a sua chave.

## Passo 4 - Mão na massa
Agora em nosso receiver PlainTextMessageReceiver vamos criar um cliente HTTP e fazer chamada na API autenticado com nossa chave obtida no passo 3.
```csharp
public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
{
    using (var client = new HttpClient())
    {
        //Obtem o conteudo da mensagem recebida pelo contato
        var query = message.Content.ToString();

        //Faz a chamada na API de busca do BING
        var queryString = $"?q={query}&mkt=pt-br";
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Startup.Settings["BingApiKey"].ToString());
        var response = await client.GetAsync(BingServiceRoot + queryString, cancellationToken);
        var result = JsonConvert.DeserializeObject<BingContainer>(await response.Content.ReadAsStringAsync());

        //Cria uma nova mensagem para responder o usuario que enviou a mensagem.
        //O [To] da messagem é o [From] da mensagem recebida
        var messageToSend = new Message
        {
            Id = EnvelopeId.NewId(),
            To = message.From
        };

        if (result.value.Count() == 0)
        {
            //Cria um conteudo somente texto para a mensagem de resposta
            messageToSend.Content = new PlainText
            {
                Text = $"Nenhuma imagem para o termo '{query}' encontrada."
            };
        }
        else
        {
            var imageResult = result.value.First();

            //Cria um conteudo de imagem para a mensagem de resposta
            messageToSend.Content = new MediaLink
            {
                PreviewUri = imageResult.thumbnailUrl != null ? new Uri(imageResult.thumbnailUrl) : null,
                //Unico campo obrigatorio
                Uri = new Uri(imageResult.contentUrl)
            };
        }

        //Responde a mensagem para o usuario
        await _sender.SendMessageAsync(messageToSend, cancellationToken);
    }
}
```

## License

[Apache 2.0 License](https://github.com/takenet/messaginghub-client-csharp/blob/master/LICENSE)
