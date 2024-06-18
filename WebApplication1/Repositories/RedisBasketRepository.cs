using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using StackExchange.Redis;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class RedisBasketRepository : IBasketRespository
{
    private readonly ILogger<RedisBasketRepository> _logger;
    private readonly IDatabase _database;
    private static RedisKey _basketKeyPrefix = "/basket/"u8.ToArray();

    public RedisBasketRepository(ILogger<RedisBasketRepository> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _database = redis.GetDatabase();
    }

    public async Task<CustomerBasket?> GetBusketAsync(string customerId)
    {
        using var data = await _database.StringGetLeaseAsync(GetBasketKey(customerId));
        if (data is null || data.Length == 0)
        {
            return null;
        }
        return JsonSerializer.Deserialize(data.Span, BasketSerializationContext.Default.CustomerBasket);
    }

    private static RedisKey GetBasketKey(string userId)
    {
        return _basketKeyPrefix.Append(userId);
    }
}

[JsonSerializable(typeof(CustomerBasket))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class BasketSerializationContext : JsonSerializerContext
{
}