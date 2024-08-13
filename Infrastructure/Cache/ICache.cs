using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Cache;

public interface ICache
{
    public IDistributedCache Cache { get; }

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    Task<bool> SetStringAsync<T>(string key, T value, int absoluteExpiration = 10, int slidingExpiration = 5, int absoluteExpirationRelativeToNow = 5);
}