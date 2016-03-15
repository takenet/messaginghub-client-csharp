using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Receivers;

namespace Buscape
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StartListenningAndWaitForCancellation(args).Wait();
        }

        private static JsonConfigFile Config;
        private static HttpClient WebClient;
        private static readonly Dictionary<Node, Tuple<string, int>> Session = new Dictionary<Node, Tuple<string, int>>();

        static async Task StartListenningAndWaitForCancellation(string[] args)
        {
            Config = ValidateAndLoadParameters(args);

            PrepareWebClient();

            await PrepareReceiverAsync();

            await WaitForUserCancellationAsync();
        }

        private static readonly MediaType ResponseMediaType = new MediaType("application", "vnd.omni.text", "json");
        private const string ConnectedMessage = "$Connected$";
        private const string StartMessage = "Iniciar";
        private const string FinishMessage = "ENCERRAR";
        private const string MoreResultsMessage = "MAIS RESULTADOS";

        private static IMessagingHubSender Sender;

        private static JsonConfigFile ValidateAndLoadParameters(string[] args)
        {
            if (args.Length > 1)
                Console.WriteLine(@"The only argument supplied must be a JSON configuration file path!");

            JsonConfigFile jsonConfigFile = null;

            var file = args.Length == 0 ? new FileInfo("config.json") : new FileInfo(args[0]);

            try
            {
                var json = File.ReadAllText(file.FullName);
                jsonConfigFile = JsonConvert.DeserializeObject<JsonConfigFile>(json);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Could not load configuration file!");
                Environment.Exit(1);
            }

            return jsonConfigFile;
        }

        private static void PrepareWebClient()
        {
            WebClient = new HttpClient();
            WebClient.DefaultRequestHeaders.Add("app-token", Config.buscapeAppToken);
        }

        private static async Task PrepareReceiverAsync()
        {
            try
            {
                Console.WriteLine(@"Trying to connect to Messaging Hub...");

                var clientBuilder = new MessagingHubClientBuilder();
                clientBuilder = clientBuilder.UsingAccessKey(Config.messagingHubApplicationShortName, Config.messagingHubApplicationAccessKey);
                clientBuilder = clientBuilder.WithSendTimeout(TimeSpan.FromSeconds(5));

                var receiverBuilder = clientBuilder.AddMessageReceiver(new MessageReceiver());

                Sender = receiverBuilder.Build();
                await Sender.StartAsync();

                //Send Message to confirm connection
                await Sender.SendMessageAsync(new Message
                {
                    To = Node.Parse(Config.messagingHubApplicationShortName),
                    Content = new PlainDocument(ConnectedMessage, MediaTypes.PlainText)
                });

                Console.WriteLine($"{DateTime.Now} -> Receiver connected to Messaging Hub!");
            }
            catch (Exception)
            {
                Console.WriteLine($"{DateTime.Now} -> Could not connect to Messaging Hub!");
            }
        }

        class MessageReceiver : IMessageReceiver
        {
            public async Task ReceiveAsync(Message envelope)
            {
                try
                {
                    await Sender.SendNotificationAsync(new Notification
                    {
                        Id = envelope.Id,
                        Event = Event.Consumed,
                        To = envelope.From
                    });

                    await ProcessMessagesAsync(envelope);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception processing message: {e}");
                    await Sender.SendMessageAsync(@"Falhou :(", envelope.From);
                }
            }
        }

        private static async Task WaitForUserCancellationAsync()
        {
            Console.ReadKey();

            Console.WriteLine($"{DateTime.Now} -> Stopping service...");

            await Sender.StopAsync();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }

        private static async Task ProcessMessagesAsync(Message message)
        {
            if (HandleChatState(message)) return;

            if (await HandleInvalidMessageTypeAsync(message)) return;

            var keyword = ((PlainText)message.Content)?.Text;

            if (HandleConnectedMessage(keyword)) return;

            if (await HandleStartMessageAsync(message, keyword)) return;

            if (await HandleEndOfSearchAsync(message, keyword)) return;

            if (await HandleNextPageRequestAsync(message, keyword)) return;

            keyword = HandleNextPageKeywordAsync(message, keyword);

            var uri = ComposeSearchUri(message, keyword);

            await ExecuteSearchAsync(message, uri);
        }

        private static async Task ExecuteSearchAsync(Message message, string uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    var buscapeResponse = await WebClient.SendAsync(request, cancellationTokenSource.Token);
                    if (buscapeResponse.StatusCode != HttpStatusCode.OK)
                    {
                        await Sender.SendMessageAsync(@"Não foi possível obter uma resposta do Buscapé!", message.From);
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
                                    await Sender.SendMessageAsync(resultItem, message.From);
                                }
                                catch (Exception e)
                                {
                                    Console.Error.WriteLine($"Exception parsing product: {e}");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            await Sender.SendMessageAsync("Nenhum resultado encontrado", message.From);
                            return;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationTokenSource.Token);
                        await Sender.SendMessageAsync($"Envie: {FinishMessage}; {MoreResultsMessage}", message.From);
                    }
                }
            }
        }

        private static string ComposeSearchUri(Message message, string keyword)
        {
            Console.WriteLine($"Requested search by {keyword}!");

            const int pageSize = 3;
            var page = 1;
            if (Session.ContainsKey(message.From))
                page = Session[message.From].Item2;

            var parameters = $"{Config.buscapeAppToken}/BR?results={pageSize}&page={page}&keyword={keyword}&format=json";
            var uri = $"http://sandbox.buscape.com.br/service/findProductList/lomadee/{parameters}";
            Session[message.From] = new Tuple<string, int>(keyword, page + 1);
            return uri;
        }

        private static string HandleNextPageKeywordAsync(Message message, string keyword)
        {
            if (Session.ContainsKey(message.From) && keyword == MoreResultsMessage)
                keyword = Session[message.From].Item1;
            return keyword;
        }

        private static async Task<bool> HandleNextPageRequestAsync(Message message, string keyword)
        {
            if (keyword != MoreResultsMessage)
                return false;

            if (Session.ContainsKey(message.From))
                return false;

            await Sender.SendMessageAsync(@"Não foi possível identificar o último item pesquisado!", message.From);
            return true;
        }

        private static async Task<bool> HandleEndOfSearchAsync(Message message, string keyword)
        {
            if (keyword == FinishMessage)
            {
                await Sender.SendMessageAsync(@"Obrigado por usar o Buscapé!", message.From);
                return true;
            }
            return false;
        }

        private static async Task<bool> HandleStartMessageAsync(Message message, string keyword)
        {
            if (keyword == StartMessage)
            {
                Console.WriteLine($"Start message received from {message.From}!");
                await Sender.SendMessageAsync(@"Tudo pronto. Qual produto deseja pesquisar?", message.From);
                return true;
            }
            return false;
        }

        private static bool HandleConnectedMessage(string keyword)
        {
            if (keyword == ConnectedMessage)
            {
                Console.WriteLine("Connected!");
                return true;
            }
            return false;
        }

        private static async Task<bool> HandleInvalidMessageTypeAsync(Message message)
        {
            if (!(message.Content is PlainText))
            {
                Console.WriteLine($"Tipo de mensagem não suportada: {message.Content.GetType().Name}!");
                await
                    Sender.SendMessageAsync("Apenas mensagens de texto são suportadas!", message.From);
                return true;
            }
            return false;
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
