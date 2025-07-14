using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Repositories.Implements
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<bool> Exists(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public Task Set(string key, string value, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_memoryCache.TryGetValue(key, out object cached) && cached is string str)
            {
                value = str;
                return true;
            }

            value = null;
            return false;
        }
    }
        
}
