using Bing;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client;
using Takenet.Textc;
using Takenet.Textc.Csdl;

namespace ImageSearch
{
    class Program
    {
        private const string LOGIN = "calendar";
        private const string ACCESS_KEY = "MnE1Mmlm";
        private const string BING_API_KEY = "1ZjkpdYqcVf5h9J1L9DCbu2I5G6iY6Fg283HSNT3LU0";
        private static readonly Uri BingServiceRoot = new Uri("https://api.datamarket.azure.com/Bing/Search/");
        private static readonly MediaType ResponseMediaType = new MediaType("application", "vnd.omni.text", "json");
        private static readonly BingSearchContainer SearchContainer = new BingSearchContainer(BingServiceRoot)
        {
            Credentials = new NetworkCredential(BING_API_KEY, BING_API_KEY)
        };

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var client = new MessagingHubClientBuilder()
                .UsingAccessKey(LOGIN, ACCESS_KEY)
                .NewTextcMessageReceiverBuilder()
                .ForSyntax("[:LDWord(mais,more) top:Integer? query+:Text]")
                    .Return<int?, string, IRequestContext, JsonDocument>((top, query, context) => GetImageDocumentAsync(query, context, top))
                .ForSyntax("[query+:Text]")
                    .Return<string, IRequestContext, JsonDocument>((query, context) => GetImageDocumentAsync(query, context))                
                .BuildAndAddTextcMessageReceiver()
                .Build();

            // Starts the client
            await client.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await client.StopAsync();
        }

        private static async Task<JsonDocument> GetImageDocumentAsync(string query, IRequestContext context, int? top = 1)
        {
            int skip;
            skip = context.GetVariable<int>(nameof(skip));

            var document = new JsonDocument(ResponseMediaType);
            var imageQuery = SearchContainer
                .Image(query, null, "pt-BR", null, null, null, null)
                .AddQueryOption($"${nameof(top)}", top)
                .AddQueryOption($"${nameof(skip)}", skip);

            var result = await Task.Factory.FromAsync(
                (c, s) => imageQuery.BeginExecute(c, s), r => imageQuery.EndExecute(r), null);

            var imageResults = result.ToList();
            if (imageResults.Count > 0)
            {
                context.SetVariable(nameof(skip), top + skip);

                var attachments = new List<IDictionary<string, object>>();

                foreach (var imageResult in imageResults)
                {
                    var attachment = new Dictionary<string, object>
                    {
                        { "mimeType", imageResult.ContentType },
                        { "mediaType", "Image" },
                        { "size", imageResult.FileSize ?? 0 },
                        { "remoteUri", imageResult.MediaUrl },
                    };

                    attachments.Add(attachment);
                }
            
                document.Add(nameof(attachments), attachments);
            }
            else
            {
                document.Add("text", $"Nenhuma imagem para o termo '{query}' encontrada.");
            }
            return document;
        }
    }
}
