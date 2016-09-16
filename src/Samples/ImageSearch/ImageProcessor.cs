using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bing;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.Textc;

namespace ImageSearch
{
    public class ImageProcessor
    {
        // Get an API key here: https://datamarket.azure.com/dataset/bing/search    
        private static readonly Uri BingServiceRoot = new Uri("https://api.datamarket.azure.com/Bing/Search/");
        private static readonly BingSearchContainer SearchContainer = new BingSearchContainer(BingServiceRoot);

        public ImageProcessor(Settings settings)
        {            
            SearchContainer.Credentials = new NetworkCredential(settings.BingApiKey, settings.BingApiKey);
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
            
            var imageQuery = SearchContainer
                .Image(query, null, "pt-BR", "Off", null, null, null)
                .AddQueryOption($"${nameof(top)}", top)
                .AddQueryOption($"${nameof(skip)}", skip);

            var result = await Task.Factory.FromAsync(
                (c, s) => imageQuery.BeginExecute(c, s), r => imageQuery.EndExecute(r), null);

            var imageResults = result.ToList();
            if (imageResults.Count == 0)
            {
                return new PlainText()
                {
                    Text = $"Nenhuma imagem para o termo '{query}' encontrada."
                };
            }

            Document document;
            if (imageResults.Count == 1)
            {                
                document = GetMediaLink(imageResults.First());
            }
            else
            {
                document = new DocumentCollection()
                {
                    Total = imageResults.Count,
                    ItemType = MediaLink.MediaType,
                    Items = imageResults.Select(GetMediaLink).Cast<Document>().ToArray()
                };
            }

            context.SetVariable(nameof(skip), top + skip);
            return document;
        }

        private static MediaLink GetMediaLink(ImageResult imageResult) => 
            new MediaLink()
            {
                Size = imageResult.FileSize,
                Type = MediaType.Parse(imageResult.ContentType),
                PreviewUri = imageResult.Thumbnail != null ? new Uri(imageResult.Thumbnail.MediaUrl) : null,
                Uri = new Uri(imageResult.MediaUrl)
            };
    }
}
