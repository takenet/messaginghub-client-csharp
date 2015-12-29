using System;
using System.Runtime.Caching;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class ContextProvider : IContextProvider, IDisposable
    {
        private readonly MemoryCache _contextCache;
        private readonly CacheItemPolicy _cacheItemPolicy;

        public ContextProvider(TimeSpan contextValidity, string cacheName = nameof(ContextProvider))
        {
            _contextCache = new MemoryCache(cacheName);
            _cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = contextValidity };
        }

        public IRequestContext GetContext(Node sender, Node destination)
        {            
            var key = $"{sender}-{destination}".ToLowerInvariant();

            var requestContext = new RequestContext();
            return
                (RequestContext)
                    (_contextCache.AddOrGetExisting(key, requestContext, _cacheItemPolicy) ?? requestContext);
        }

        public void Dispose()
        {
            _contextCache.Dispose();
        }
    }
}