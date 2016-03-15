using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Buscape
{
    public class MessageReceiver : MessageReceiverBase
    {
        private const string StartMessage = "Iniciar";
        private const string FinishMessage = "ENCERRAR";
        private const string MoreResultsMessage = "MAIS RESULTADOS";

        private static readonly MediaType ResponseMediaType = new MediaType("application", "vnd.omni.text", "json");

        private IDictionary<string, object> Settings { get; }

        private HttpClient _webClient;
        private HttpClient WebClient
        {
            get
            {
                if (_webClient == null)
                {
                    _webClient = new HttpClient();
                    _webClient.DefaultRequestHeaders.Add("app-token", Settings["buscapeAppToken"].ToString());
                }
                return _webClient;
            }
        }

        private readonly Dictionary<Node, Tuple<string, int>> Session = new Dictionary<Node, Tuple<string, int>>();

        private MessageReceiver(IDictionary<string, object> settings)
        {
            Settings = settings;
        }

        public override async Task ReceiveAsync(Message message)
        {
            try
            {
                await EnvelopeSender.SendNotificationAsync(new Notification
                {
                    Id = message.Id,
                    Event = Event.Consumed,
                    To = message.From
                });

                await ProcessMessagesAsync(message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Exception processing message: {e}");
                await EnvelopeSender.SendMessageAsync(@"Falhou :(", message.From);
            }
        }

        private async Task ProcessMessagesAsync(Message message)
        {
            if (HandleChatState(message)) return;

            if (await HandleInvalidMessageTypeAsync(message)) return;

            var keyword = ((PlainText)message.Content)?.Text;

            if (await HandleStartMessageAsync(message, keyword)) return;

            if (await HandleEndOfSearchAsync(message, keyword)) return;

            if (await HandleNextPageRequestAsync(message, keyword)) return;

            keyword = HandleNextPageKeywordAsync(message, keyword);

            var uri = ComposeSearchUri(message, keyword);

            await ExecuteSearchAsync(message, uri);
        }

        private static bool HandleChatState(Message message)
        {
            var chatState = message.Content as ChatState;
            if (chatState != null)
            {
                Console.WriteLine($"ChatState received and ignored: {chatState}");
                return true;
            }
            return false;
        }

        private async Task<bool> HandleInvalidMessageTypeAsync(Message message)
        {
            if (!(message.Content is PlainText))
            {
                Console.WriteLine($"Tipo de mensagem não suportada: {message.Content.GetType().Name}!");
                await
                    EnvelopeSender.SendMessageAsync("Apenas mensagens de texto são suportadas!", message.From);
                return true;
            }
            return false;
        }

        private async Task<bool> HandleStartMessageAsync(Message message, string keyword)
        {
            if (keyword == StartMessage)
            {
                Console.WriteLine($"Start message received from {message.From}!");
                await EnvelopeSender.SendMessageAsync(@"Tudo pronto. Qual produto deseja pesquisar?", message.From);
                return true;
            }
            return false;
        }

        private async Task<bool> HandleEndOfSearchAsync(Message message, string keyword)
        {
            if (keyword == FinishMessage)
            {
                await EnvelopeSender.SendMessageAsync(@"Obrigado por usar o Buscapé!", message.From);
                return true;
            }
            return false;
        }

        private async Task<bool> HandleNextPageRequestAsync(Message message, string keyword)
        {
            if (keyword != MoreResultsMessage)
                return false;

            if (Session.ContainsKey(message.From))
                return false;

            await EnvelopeSender.SendMessageAsync(@"Não foi possível identificar o último item pesquisado!", message.From);
            return true;
        }

        private string HandleNextPageKeywordAsync(Message message, string keyword)
        {
            if (Session.ContainsKey(message.From) && keyword == MoreResultsMessage)
                keyword = Session[message.From].Item1;
            return keyword;
        }

        private string ComposeSearchUri(Message message, string keyword)
        {
            Console.WriteLine($"Requested search by {keyword}!");

            const int pageSize = 3;
            var page = 1;
            if (Session.ContainsKey(message.From))
                page = Session[message.From].Item2;

            var parameters = $"{Settings["buscapeAppToken"]}/BR?results={pageSize}&page={page}&keyword={keyword}&format=json";
            var uri = $"http://sandbox.buscape.com.br/service/findProductList/lomadee/{parameters}";
            Session[message.From] = new Tuple<string, int>(keyword, page + 1);
            return uri;
        }

        private async Task ExecuteSearchAsync(Message message, string uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    var buscapeResponse = await WebClient.SendAsync(request, cancellationTokenSource.Token);
                    if (buscapeResponse.StatusCode != HttpStatusCode.OK)
                    {
                        await EnvelopeSender.SendMessageAsync(@"Não foi possível obter uma resposta do Buscapé!", message.From);
                    }
                    else
                    {
                        var resultJson = await buscapeResponse.Content.ReadAsStringAsync();
                        dynamic responseMessage = JsonConvert.DeserializeObject(resultJson);
                        try
                        {
                            foreach (JObject product in responseMessage.product)
                            {
                                try
                                {
                                    var resultItem = ParseProduct(product);
                                    await EnvelopeSender.SendMessageAsync(resultItem, message.From);
                                }
                                catch (Exception e)
                                {
                                    Console.Error.WriteLine($"Exception parsing product: {e}");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            await EnvelopeSender.SendMessageAsync("Nenhum resultado encontrado", message.From);
                            return;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationTokenSource.Token);
                        await EnvelopeSender.SendMessageAsync($"Envie: {FinishMessage}; {MoreResultsMessage}", message.From);
                    }
                }
            }
        }

        private static Document ParseProduct(JObject product)
        {
            var obj = product.Properties().First();
            var name = obj.Value["productname"]?.Value<string>() ??
                       obj.Value["productshortname"]?.Value<string>() ?? "Produto Desconhecido!";
            var pricemin = obj.Value["pricemin"]?.Value<string>();
            var pricemax = obj.Value["pricemax"]?.Value<string>();
            var text = name;
            if (pricemin != null && pricemax != null)
                text += $"\nDe R$ {pricemin} a R$ {pricemax}.";
            var thumbnail =
                obj.Value["thumbnail"]["formats"].Single(
                    f => f["formats"]["width"].Value<int>() == 100)["formats"]["url"]
                    .Value<string>();
            var link =
                obj.Value["links"].Single(
                    l => l["link"]["type"].Value<string>() == "product")["link"]["url"]
                    .Value<string>();
            var resultItem = BuildMessage(thumbnail, text, link);
            return resultItem;
        }

        private static Document BuildMessage(string imageUri, string text, string link)
        {
            if (imageUri == null)
            {
                return new PlainText
                {
                    Text = link != null ? $"{text}\n{link}" : text
                };
            }

            var document = new JsonDocument(ResponseMediaType)
            {
                {
                    nameof(text), link != null ? $"{text}\n{link}" : text
                }
            };

            var attachments = new List<IDictionary<string, object>>();

            var attachment = new Dictionary<string, object>
            {
                {"mimeType", "image/jpeg"},
                {"mediaType", "image"},
                {"size", 100},
                {"remoteUri", imageUri},
                {"thumbnailUri", imageUri}
            };
            attachments.Add(attachment);

            document.Add(nameof(attachments), attachments);

            return document;
        }
    }
}
