using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Extensions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder builder, Action<JsonSerializerOptions> configure)
    {
        builder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            configure(o.JsonSerializerOptions);
        });

        return builder;
    }


    public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(this IEventBusBuilder builder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        builder.Services.AddKeyedTransient<IIntegrationEventHandler, TH>(typeof(T));
        builder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            o.EventTypes[typeof(T).Name] = typeof(T);
        });

        return builder;
    }
}
