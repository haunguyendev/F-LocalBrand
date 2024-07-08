
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SWD.F_LocalBrand.Business.Attributes
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ResponseCacheService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
        }
        public async Task SetCacheResponseAsync(string cacheKey, object response, TimeSpan timeOut)
        {
            if(response == null)
            {
                return;
            }
            var serializedResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await _distributedCache.SetStringAsync(cacheKey, serializedResponse, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeOut
            });
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        { 
            var cachedResponse = await _distributedCache.GetStringAsync(cacheKey);
            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }

        public async Task RemoveCacheRepsonseAsync(string pattern)
        {
            if(string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Value can not be null or whitespace");
            }
            await foreach(var key in GetKeysAsync(pattern + "*"))
            {
                await _distributedCache.RemoveAsync(key);
            }
        }

        private async IAsyncEnumerable<string> GetKeysAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Value can not be null or whitespace");
            }
            foreach(var endPoint in _connectionMultiplexer.GetEndPoints())
            {
                var server = _connectionMultiplexer.GetServer(endPoint);
                //var serverKeys = server.Keys(pattern: pattern);
                foreach (var key in server.Keys(pattern: pattern))
                {
                    yield return key.ToString();
                }
            }
        }
    }
}
