using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using System.Diagnostics;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Lime.Messaging.Contents;
using System.Net.Http;
using Newtonsoft.Json;

namespace BingImageSearch
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;
        private static readonly Uri BingServiceRoot = new Uri("https://api.cognitive.microsoft.com/bing/v5.0/images/search");

        public PlainTextMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
        }

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
    }
}
