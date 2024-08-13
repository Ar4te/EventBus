using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Infrastructure.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache;

[InjectLeftTime(ServiceLifetime.Singleton)]
public class RedisCache : ICache
{
    private readonly IDistributedCache _cache;

    public RedisCache(IDistributedCache cache)
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

public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key, int timeout, CancellationToken cancellationToken);
}
public class CacheManager : ICacheManager
{
    private readonly RedisCache _redisCache;
    private readonly MemoryCache _memoryCache;
    private readonly ILogger<CacheManager> _logger;

    public CacheManager(ICache cache, ILogger<CacheManager> logger)
    {
        _redisCache = cache as RedisCache ?? throw new InvalidCastException(nameof(RedisCache));
        _memoryCache = cache as MemoryCache ?? throw new InvalidCastException(nameof(MemoryCache));
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, int timeout, CancellationToken cancellationToken)
    {
        Before();
        _logger.LogInformation("Starting GetAsync for key: {Key}", key);

        var stopWatch = Stopwatch.StartNew();

        // 优先从 RedisCache 获取数据
        T? res = await _redisCache.GetAsync<T>(key, cancellationToken);
        if (res?.Equals(default(T)) == false)
        {
            _logger.LogInformation("Cache hit in Redis for key: {Key}", key);
            stopWatch.Stop();
            _logger.LogInformation("Finished GetAsync for key: {Key}, time usage: {Diff}ms", key, stopWatch.ElapsedMilliseconds);
            return res;
        }

        // 如果 RedisCache 未命中，则从 MemoryCache 获取数据
        _logger.LogInformation("Cache miss in Redis for key: {Key}. Trying MemoryCache.", key);
        res = await _memoryCache.GetAsync<T>(key, cancellationToken);
        if (res?.Equals(default(T)) == false)
        {
            _logger.LogInformation("Cache hit in MemoryCache for key: {Key}", key);
        }
        else
        {
            _logger.LogWarning("Cache miss in MemoryCache for key: {Key}", key);
        }

        stopWatch.Stop();
        _logger.LogInformation("Finished GetAsync for key: {Key}, time usage: {Diff}ms", key, stopWatch.ElapsedMilliseconds);
        After();
        return res;
    }

    private void Before()
    {
        // Before
    }

    private void After()
    {
        // After
    }
}