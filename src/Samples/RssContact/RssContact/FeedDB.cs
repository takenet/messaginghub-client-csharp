using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace RssContact
{
    public class FeedDB
    {
        static ConcurrentDictionary<string, HashSet<Uri>> _db = new ConcurrentDictionary<string, HashSet<Uri>>();

        public Task AddFeedAsync(string user, Uri uri)
        {
            var feeds = _db.GetOrAdd(user, _ => new HashSet<Uri>());
            feeds.Add(uri);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Uri>> GetFeedsAsync(string user)
        {
            HashSet<Uri> feeds;
            if (!_db.TryGetValue(user, out feeds))
            {
                feeds = new HashSet<Uri>();
            }

            return Task.FromResult(feeds.AsEnumerable());
        }

        public Task<IEnumerable<FeedItem>> GetLatestAsync(string user)
        {
            HashSet<Uri> feeds;
            if (!_db.TryGetValue(user, out feeds))
            {
                feeds = new HashSet<Uri>();
            }

            var items = feeds
                        .SelectMany(FeedParser.Get)
                        .OrderBy(i => i.PublishDate)
                        .ToArray();

            return Task.FromResult(items.AsEnumerable());
        }


    }
}
