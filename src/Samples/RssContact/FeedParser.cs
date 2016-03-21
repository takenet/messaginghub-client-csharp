using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RssContact
{
    public class FeedParser
    {
        public static IEnumerable<FeedItem> Get(Uri uri)
        {
            using (var reader = XmlReader.Create(uri.ToString()))
            {
                var feed = SyndicationFeed.Load(reader);
                return feed.Items.Select(i => new FeedItem
                {
                    Title = i.Title.Text,
                    Content = i.Summary.Text,
                    PublishDate = i.PublishDate
                });
            }
        }
    }
}
