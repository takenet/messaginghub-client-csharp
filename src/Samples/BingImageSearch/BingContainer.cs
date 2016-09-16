using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Services.Client;
using System.Net;
using System.IO;

namespace BingImageSearch
{

    public class BingContainer
    {
        public string _type { get; set; }
        public Instrumentation instrumentation { get; set; }
        public string webSearchUrl { get; set; }
        public int totalEstimatedMatches { get; set; }
        public Value[] value { get; set; }
        public Queryexpansion[] queryExpansions { get; set; }
        public int nextOffsetAddCount { get; set; }
        public Pivotsuggestion[] pivotSuggestions { get; set; }
        public bool displayShoppingSourcesBadges { get; set; }
        public bool displayRecipeSourcesBadges { get; set; }
    }

    public class Instrumentation
    {
        public string pageLoadPingUrl { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public string webSearchUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string datePublished { get; set; }
        public string contentUrl { get; set; }
        public string hostPageUrl { get; set; }
        public string contentSize { get; set; }
        public string encodingFormat { get; set; }
        public string hostPageDisplayUrl { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Thumbnail thumbnail { get; set; }
        public string imageInsightsToken { get; set; }
        public Insightssourcessummary insightsSourcesSummary { get; set; }
        public string imageId { get; set; }
        public string accentColor { get; set; }
    }

    public class Thumbnail
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Insightssourcessummary
    {
        public int shoppingSourcesCount { get; set; }
        public int recipeSourcesCount { get; set; }
    }

    public class Queryexpansion
    {
        public string text { get; set; }
        public string displayText { get; set; }
        public string webSearchUrl { get; set; }
        public string searchLink { get; set; }
        public Thumbnail1 thumbnail { get; set; }
    }

    public class Thumbnail1
    {
        public string thumbnailUrl { get; set; }
    }

    public class Pivotsuggestion
    {
        public string pivot { get; set; }
        public Suggestion[] suggestions { get; set; }
    }

    public class Suggestion
    {
        public string text { get; set; }
        public string displayText { get; set; }
        public string webSearchUrl { get; set; }
        public string searchLink { get; set; }
        public Thumbnail2 thumbnail { get; set; }
    }

    public class Thumbnail2
    {
        public string thumbnailUrl { get; set; }
    }

}
