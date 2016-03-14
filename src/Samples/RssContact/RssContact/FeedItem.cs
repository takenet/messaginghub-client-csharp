using System;

namespace RssContact
{
    public class FeedItem
    {
        public string Title { get; internal set; }
        public string Content { get; internal set; }
        public DateTimeOffset PublishDate { get; internal set; }
        public string Link { get; internal set; }
    }
}
