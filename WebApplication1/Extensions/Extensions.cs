using System.Text.Json.Serialization;
using EventBus.Extensions;
using EventBus.RabbitMQ;
using WebApplication1.Apis;
using WebApplication1.IntegrationEvents.Events;

namespace WebApplication1.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        //builder.AddRedisClient("redis");
        //builder.Services.AddSingleton<IBasketRespository, RedisBasketRepository>();
        builder.AddRabbitMqEventBus()
            .AddSubscription<Tes32tEvent, TestEventHandler>()
            .AddSubscription<Tes32tEvent, TestEventHandler2>()
            .ConfigureJsonOptions(opt => opt.TypeInfoResolverChain.Add(IntegrationEventContext.Default));
    }
}

[JsonSerializable(typeof(OrderStartedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{

}
