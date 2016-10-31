using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Textc;

namespace Takenet.MessagingHub.Client.Textc
{
    public sealed class ContextProvider : IContextProvider, IDisposable
    {
        private readonly ICultureProvider _cultureProvider;
        private readonly MemoryCache _contextCache;
        private readonly CacheItemPolicy _cacheItemPolicy;

        public ContextProvider(ICultureProvider cultureProvider, TimeSpan contextValidity, string cacheName = nameof(ContextProvider))
        {
            if (cultureProvider == null) throw new ArgumentNullException(nameof(cultureProvider));
            _cultureProvider = cultureProvider;
            _contextCache = new MemoryCache(cacheName);
            _cacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = contextValidity };
        }

        public Task<IRequestContext> GetContextAsync(Node sender, Node destination)
        {
            var key = GetKey(sender, destination);
            var requestContext = new RequestContext();

            return Task.FromResult<IRequestContext>(
                (RequestContext)(_contextCache.AddOrGetExisting(key, requestContext, _cacheItemPolicy) ?? requestContext));
        }

        public Task SaveContextAsync(Node sender, Node destination, IRequestContext context)
        {
            var key = GetKey(sender, destination);
            _contextCache.Set(key, context, _cacheItemPolicy);
            return Task.CompletedTask;
        }

        private static string GetKey(Node sender, Node destination)
        {
            var key = $"{sender}-{destination}".ToLowerInvariant();
            return key;
        }


        public void Dispose()
        {
            _contextCache.Dispose();
        }
    }
}