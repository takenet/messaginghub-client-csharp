using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bing;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Textc;
using Takenet.Textc;

namespace ImageSearch
{
    public class ImageProcessor
    {
        // Get an API key here: https://datamarket.azure.com/dataset/bing/search    
        private static readonly Uri BingServiceRoot = new Uri("https://api.datamarket.azure.com/Bing/Search/");
        private static readonly MediaType ResponseMediaType = new MediaType("application", "vnd.omni.text", "json");
        private static readonly BingSearchContainer SearchContainer = new BingSearchContainer(BingServiceRoot);

        public ImageProcessor(IDictionary<string, object> settings)
        {
            var bingApiKey = (string) settings["bingApiKey"];
            SearchContainer.Credentials = new NetworkCredential(bingApiKey, bingApiKey);
        }

        public static async Task<Document> GetFirstImageDocumentAsync(string query, IRequestContext context)
        {
            var lastQuery = context.GetVariable<string>(nameof(query));
            if (lastQuery == null || !lastQuery.Equals(query))
            {
                context.RemoveVariable("skip");                
            }
            return await GetImageDocumentAsync(1, query, context);
        }

        public static async Task<Document> GetImageDocumentAsync(int? top, string query, IRequestContext context)
        {
            if (top == null) top = 1;
            int skip;
            skip = context.GetVariable<int>(nameof(skip));

            context.SetVariable(nameof(query), query);

            Document document;
            var imageQuery = SearchContainer
                .Image(query, null, "pt-BR", "Off", null, null, null)
                .AddQueryOption($"${nameof(top)}", top)
                .AddQueryOption($"${nameof(skip)}", skip);

            var result = await Task.Factory.FromAsync(
                (c, s) => imageQuery.BeginExecute(c, s), r => imageQuery.EndExecute(r), null);

            var from = context.GetMessageFrom();

            var imageResults = result.ToList();
            if (imageResults.Count > 0)
            {
                context.SetVariable(nameof(skip), top + skip);

                if (from.Domain.Equals("0mn.io"))
                {
                    document = new JsonDocument(ResponseMediaType);
                    var attachments = new List<IDictionary<string, object>>();

                    foreach (var imageResult in imageResults)
                    {
                        var attachment = new Dictionary<string, object>
                        {
                            {"mimeType", imageResult.ContentType},
                            {"mediaType", "image"},
                            {"size", imageResult.FileSize ?? 0},
                            {"remoteUri", imageResult.MediaUrl},
                            {"thumbnailUri", imageResult.Thumbnail?.MediaUrl}
                        };
                        attachments.Add(attachment);
                    }

                    ((JsonDocument)document).Add(nameof(attachments), attachments);
                }
                else
                {
                    var imageResult = imageResults.First();

                    document = new MediaLink()
                    {
                        Size = imageResult.FileSize,
                        Type = MediaType.Parse(imageResult.ContentType),
                        PreviewUri = imageResult.Thumbnail != null ? new Uri(imageResult.Thumbnail.MediaUrl) : null,
                        Uri = new Uri(imageResult.MediaUrl)
                    };
                }
            }
            else
            {
                document = new PlainText()
                {
                    Text = $"Nenhuma imagem para o termo '{query}' encontrada."
                };
            }
            return document;
        }
    }
}
