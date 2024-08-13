using System.Text;
using System.Text.Json;
using Infrastructure.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cache;

[InjectLeftTime(ServiceLifetime.Singleton)]
public class MemoryCache : ICache
{
    private readonly IDistributedCache _cache;

    public MemoryCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public IDistributedCache Cache => _cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var bytes = await Cache.GetAsync(key, cancellationToken);
        if (bytes?.Length != 0)
        {
            var str = Encoding.UTF8.GetString(bytes!);
            try
            {
                var tVal = JsonSerializer.Deserialize<T>(str);
                return tVal;
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    public async Task<bool> SetStringAsync<T>(string key, T value, int absoluteExpiration = 10, int slidingExpiration = 5, int absoluteExpirationRelativeToNow = 5)
    {
        try
        {
            var strVal = JsonSerializer.Serialize(value, typeof(T));
            await Cache.SetStringAsync(key, strVal, new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(absoluteExpiration),
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteExpirationRelativeToNow)
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}