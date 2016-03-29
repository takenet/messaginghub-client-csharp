using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bing;
using Lime.Protocol;
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

        public async Task<JsonDocument> GetFirstImageDocumentAsync(string query, IRequestContext context)
        {
            var lastQuery = context.GetVariable<string>(nameof(query));
            if (lastQuery == null || !lastQuery.Equals(query))
            {
                context.RemoveVariable("skip");                
            }
            return await GetImageDocumentAsync(1, query, context);
        }

        public async Task<JsonDocument> GetImageDocumentAsync(int? top, string query, IRequestContext context)
        {
            if (top == null) top = 1;
            int skip;
            skip = context.GetVariable<int>(nameof(skip));

            context.SetVariable(nameof(query), query);

            var document = new JsonDocument(ResponseMediaType);
            var imageQuery = SearchContainer
                .Image(query, null, "pt-BR", "Off", null, null, null)
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
                        { "mediaType", "image" },
                        { "size", imageResult.FileSize ?? 0 },
                        { "remoteUri", imageResult.MediaUrl },
                        { "thumbnailUri", imageResult.Thumbnail?.MediaUrl }
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
