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

namespace Buscape
{
    class Program
    {
        static void Main(string[] args)
        {
            StartListenningAndWaitForCancellation(args).Wait();
        }

        static async Task StartListenningAndWaitForCancellation(string[] args)
        {
            var config = ValidateAndLoadParameters(args);

            var webClient = PrepareWebClient(config.buscapeAppToken);

            var receiver = await PrepareReceiverAsync(config.messagingHubApplicationShortName, config.messagingHubApplicationAccessKey);

            Console.WriteLine();
            Console.WriteLine(@"Press any key to continue...");
            Console.ReadKey();

            if (receiver == null)
                return;

            using (var cts = new CancellationTokenSource())
            {

                StartReceivingMessages(cts, receiver, webClient, config.buscapeAppToken);

                await WaitForUserCancellationAsync(cts, receiver);
            }
        }

        private const string ConnectedMessage = "$Connected$";
        private const string StartMessage = "Iniciar";
        private static readonly List<Guid> ProcessedMessageIds = new List<Guid>();

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

        private static HttpClient PrepareWebClient(string appToken)
        {
            var webClient = new HttpClient();
            webClient.DefaultRequestHeaders.Add("app-token", appToken);

            return webClient;
        }

        private static async Task<IMessagingHubClient> PrepareReceiverAsync(string appShortName, string accessKey)
        {
            try
            {
                Console.WriteLine(@"Trying to connect to Messaging Hub...");

                var receiverBuilder = new MessagingHubClientBuilder();
                receiverBuilder = receiverBuilder.UsingAccessKey(appShortName, accessKey);
                receiverBuilder = receiverBuilder.WithSendTimeout(TimeSpan.FromSeconds(30));
                var receiver = receiverBuilder.Build();
                await receiver.StartAsync();

                //Send Message to confirm connection
                await receiver.SendMessageAsync(new Message
                {
                    To = Node.Parse(appShortName),
                    Content = new PlainDocument(ConnectedMessage, MediaTypes.PlainText)
                });

                Console.WriteLine($"{DateTime.Now} -> Receiver connected to Messaging Hub!");

                return receiver;
            }
            catch (Exception)
            {
                Console.WriteLine($"{DateTime.Now} -> Could not connect to Messaging Hub!");
                return null;
            }
        }

        private static async Task WaitForUserCancellationAsync(CancellationTokenSource cts, IMessagingHubClient receiver)
        {
            Console.ReadKey();

            Console.WriteLine($"{DateTime.Now} -> Stopping service...");

            cts.Cancel();

            await receiver.StopAsync();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }

        private static void StartReceivingMessages(CancellationTokenSource cts, IMessagingHubClient receiver, HttpClient webClient, string appToken)
        {
            receiver.StartReceivingMessages(cts, async message =>
            {
                if (ProcessedMessageIds.Contains(message.Id))
                {
                    Console.WriteLine($"Repeated message received and ignored: {message.Id}");
                    return;
                }

                ProcessedMessageIds.Add(message.Id);

                try
                {
                    var chatState = message.Content as ChatState;
                    if (chatState != null) { 
                        Console.WriteLine($"ChatState received and ignored: {chatState}");
                        return;
                    }

                    if (!(message.Content is PlainText))
                    {
                        Console.WriteLine($"Tipo de mensagem não suportada: {message.Content.GetType().Name}!");
                        await receiver.SendMessageAsync($"Tipo de mensagem não suportada: {message.Content.GetType().Name}!", message.From);
                        return;
                    }

                    var keyword = ((PlainText)message.Content)?.Text;

                    if (keyword == ConnectedMessage)
                    {
                        Console.WriteLine("Connected!");
                    }
                    else if (keyword == StartMessage)
                    {
                        Console.WriteLine($"Start message received from {message.From}!");
                        await receiver.SendMessageAsync(@"Tudo pronto. Qual produto deseja pesquisar?", message.From);
                    }
                    else
                    {
                        Console.WriteLine($"Requested search by {keyword}!");
                        var uri =
                            $"http://sandbox.buscape.com.br/service/findProductList/lomadee/{appToken}/BR?results=10&page=1&keyword={keyword}&format=json";

                        using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                        {
                            var response = await webClient.SendAsync(request, cts.Token);
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                await receiver.SendMessageAsync(@"Não foi possível obter uma resposta do Buscapé!", message.From);
                            }
                            else
                            {
                                var resultJson = await response.Content.ReadAsStringAsync();
                                dynamic responseMessage = JsonConvert.DeserializeObject(resultJson);
                                foreach (JObject product in responseMessage.product)
                                {
                                    try
                                    {
                                        var obj = product.Properties().First();
                                        var name = obj.Value["productshortname"].Value<string>();
                                        var pricemin = obj.Value["pricemin"].Value<string>();
                                        var pricemax = obj.Value["pricemax"].Value<string>();
                                        await
                                            receiver.SendMessageAsync($"{name} de {pricemin} até {pricemax}", message.From);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.Error.WriteLine($"Exception parsing product: {e}");
                                    }
                                }
                                await receiver.SendMessageAsync(@"Pronto para um nova pesquisa!", message.From);
                            }
                        }
                    }

                    await receiver.SendNotificationAsync(new Notification
                    {
                        Id = message.Id,
                        Event = Event.Consumed,
                        To = message.From
                    });
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception processing message: {e}");
                    await receiver.SendMessageAsync(@"Falhou :(", message.From);
                }
            });
            Console.WriteLine(@"Listening...");
        }
    }
}
