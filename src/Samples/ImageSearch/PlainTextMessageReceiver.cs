using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Bing;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Lime.Messaging.Contents;

namespace ImageSearch
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        // Obtenha chave da API aqui: https://datamarket.azure.com/dataset/bing/search    
        private static readonly Uri BingServiceRoot = new Uri("https://api.datamarket.azure.com/Bing/Search/");
        private static readonly BingSearchContainer SearchContainer = new BingSearchContainer(BingServiceRoot);

        private readonly IMessagingHubSender _sender;

        public PlainTextMessageReceiver(IMessagingHubSender sender,
            IDictionary<string, string> settings)
        {
            _sender = sender;
            SearchContainer.Credentials = new NetworkCredential(settings["BingApiKey"], settings["BingApiKey"]);
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Obtem o conteudo da mensagem recebida pelo contato
            var query = message.Content.ToString();

            //Busca na api do Bing as imagens correspondente ao texto enviado
            var imageQuery = SearchContainer
                .Image(query, null, "pt-BR", "Off", null, null, null);
            var result = await Task.Factory.FromAsync(
               (c, s) => imageQuery.BeginExecute(c, s), r => imageQuery.EndExecute(r), null);
            var imageResults = result.ToList();

            //Cria uma nova mensagem para responder o usuario que enviou a mensagem.
            //O [To] da messagem é o [From] da mensagem recebida
            var messageToSend = new Message
            {
                Id = EnvelopeId.NewId(),
                To = message.From
            };

            if (imageResults.Count == 0)
            {
                //Cria um conteudo somente texto para a mensagem de resposta
                messageToSend.Content = new PlainText
                {
                    Text = $"Nenhuma imagem para o termo '{query}' encontrada."
                };
            }
            else
            {
                var imageResult = imageResults.First();

                //Cria um conteudo de imagem para a mensagem de resposta
                messageToSend.Content = new MediaLink
                {
                    Size = imageResult.FileSize,
                    Type = MediaType.Parse(imageResult.ContentType),
                    PreviewUri = imageResult.Thumbnail != null ? new Uri(imageResult.Thumbnail.MediaUrl) : null,
                    //Unico campo obrigatorio
                    Uri = new Uri(imageResult.MediaUrl)
                };
            }

            //Responde a mensagem para o usuario
            await _sender.SendMessageAsync(messageToSend, cancellationToken);
        }
    }
}
