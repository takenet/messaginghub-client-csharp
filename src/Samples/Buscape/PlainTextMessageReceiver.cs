using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Buscape
{
    public sealed class PlainTextMessageReceiver : MessageReceiverBase, IDisposable
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

        private readonly MemoryCache Session = new MemoryCache(nameof(Buscape));

        public PlainTextMessageReceiver(IDictionary<string, object> settings)
        {
            Settings = settings;
            
        }

        public override async Task ReceiveAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            try
            {
                await ProcessMessagesAsync(sender, message, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception processing message: {e}");
                await sender.SendMessageAsync(@"Falhou :(", message.From, cancellationToken);
            }
        }

        private async Task ProcessMessagesAsync(IMessagingHubSender sender, Message message, CancellationToken cancellationToken)
        {
            var keyword = message.Content.ToString();

            if (keyword.First() == '#')
                keyword = keyword.Substring(Math.Min(keyword.Length, 5)).Trim();

            if (await HandleStartMessageAsync(sender, message, keyword, cancellationToken)) return;

            if (await HandleEndOfSearchAsync(sender, message, keyword, cancellationToken)) return;

            if (await HandleNextPageRequestAsync(sender, message, keyword, cancellationToken)) return;

            keyword = HandleNextPageKeywordAsync(message, keyword);

            var uri = await ComposeSearchUriAsync(message, keyword);

            await ExecuteSearchAsync(sender, message, uri, cancellationToken);
        }

        private static async Task<bool> HandleStartMessageAsync(IMessagingHubSender sender, Message message, string keyword, CancellationToken cancellationToken)
        {
            if (keyword != StartMessage)
                return false;

            Console.WriteLine($"Start message received from {message.From}!");
            await sender.SendMessageAsync(@"Tudo pronto. Qual produto deseja pesquisar?", message.From, cancellationToken);
            return true;
        }

        private static async Task<bool> HandleEndOfSearchAsync(IMessagingHubSender sender, Message message, string keyword, CancellationToken cancellationToken)
        {
            if (keyword != FinishMessage)
                return false;

            await sender.SendMessageAsync(@"Obrigado por usar o aplicativo OMNI!", message.From, cancellationToken);
            return true;
        }

        private async Task<bool> HandleNextPageRequestAsync(IMessagingHubSender sender, Message message, string keyword, CancellationToken cancellationToken)
        {
            if (keyword != MoreResultsMessage)
                return false;

            if (Session.Contains(message.From.ToString()))
                return false;

            await sender.SendMessageAsync(@"Não foi possível identificar o último item pesquisado!", message.From, cancellationToken);
            return true;
        }

        private string HandleNextPageKeywordAsync(Message message, string keyword)
        {
            if (Session.Contains(message.From.ToString()) && keyword == MoreResultsMessage)
                keyword = ((Tuple<string, int>)Session[message.From.ToString()]).Item1;
            return keyword;
        }

        private async Task<string> ComposeSearchUriAsync(Message message, string keyword)
        {
            Console.WriteLine($"Requested search by {keyword}!");

            var originalKeyword = keyword;

            var categoryId = await DecodeCategoryIdAsync(keyword);
            if (categoryId > 0)
                keyword = string.Join("+", keyword.Split(' ').Skip(1));

            const int pageSize = 3;
            var page = 1;
            if (Session.Contains(message.From.ToString()))
                page = ((Tuple<string, int>)Session[message.From.ToString()]).Item2;

            var parameters = $"{Settings["buscapeAppToken"]}/BR?results={pageSize}&page={page}&keyword={keyword}&format=json&sort=price";
            if (categoryId > 0)
                parameters += $"&categoryId={categoryId}";

            var uri = $"http://sandbox.buscape.com.br/service/findProductList/{parameters}";

            if (Session.Contains(message.From.ToString()))
                Session.Remove(message.From.ToString());

            Session.Add(message.From.ToString(), new Tuple<string, int>(originalKeyword, page + 1), new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
            });

            return uri;
        }

        private async Task<int> DecodeCategoryIdAsync(string keyword)
        {
            try
            {
                var keywords = keyword.Split(' ');
                if (keywords.Length > 1)
                {
                    var category = keywords[0];
                    var categoryParameters = $"{Settings["buscapeAppToken"]}/BR?keyword={category}&format=json";
                    var categoryUri = $"http://sandbox.buscape.com.br/service/findCategoryList/{categoryParameters}";
                    using (var request = new HttpRequestMessage(HttpMethod.Get, categoryUri))
                    {
                        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                        {
                            var buscapeResponse = await WebClient.SendAsync(request, cancellationTokenSource.Token);
                            if (buscapeResponse.StatusCode != HttpStatusCode.OK)
                                return 0;
                            var resultJson = await buscapeResponse.Content.ReadAsStringAsync();
                            var responseMessage = JsonConvert.DeserializeObject<JObject>(resultJson);
                            return responseMessage["subcategory"][0]["subcategory"]["id"].Value<int>();
                        }
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private async Task ExecuteSearchAsync(IMessagingHubSender sender, Message message, string uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    var buscapeResponse = await WebClient.SendAsync(request, cancellationTokenSource.Token);
                    if (buscapeResponse.StatusCode != HttpStatusCode.OK)
                    {
                        await sender.SendMessageAsync(@"Não foi possível obter uma resposta do Buscapé!", message.From, cancellationToken);
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
                                    await sender.SendMessageAsync(resultItem, message.From, cancellationToken);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Exception parsing product: {e}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception parsing response from Buscapé: {e}");
                            await sender.SendMessageAsync("Nenhum resultado encontrado", message.From, cancellationToken);
                            return;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationTokenSource.Token);
                        await sender.SendMessageAsync($"Envie: {FinishMessage}; {MoreResultsMessage}", message.From, cancellationToken);
                        Console.WriteLine("Response sent!");
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

        public void Dispose()
        {
            Session.Dispose();
        }
    }
}
