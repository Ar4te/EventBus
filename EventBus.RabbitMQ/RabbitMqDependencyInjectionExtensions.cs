using EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public static class RabbitMqDependencyInjectionExtensions
{
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...",
    //     "RetryCount": 10
    //   }
    // }

    private const string SectionName = "EventBus:SubcribeOptions";

    public static IEventBusBuilder AddRabbitMqEventBusWithAspire(this IHostApplicationBuilder appBuilder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);
        appBuilder.AddRabbitMQClient(connectionName, configureConnectionFactory: factory =>
        {
            ((ConnectionFactory)factory).DispatchConsumersAsync = true;
        });

        appBuilder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
            });

        appBuilder.Services.Configure<EventBusOptions>(appBuilder.Configuration.GetSection(SectionName));
        appBuilder.Services.AddSingleton<RabbitMQTelemetry>();
        appBuilder.Services.AddSingleton<IEventBus, RabbitMQEventBusWithAspire>();
        appBuilder.Services.AddSingleton<IHostedService>(sp => (RabbitMQEventBusWithAspire)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(appBuilder.Services);
    }

    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder appBuilder, string? subscribeSectionName = null)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);

        appBuilder.AddRabbitMQClient(configureConnectionFactory: factory =>
        {
            ((ConnectionFactory)factory).DispatchConsumersAsync = true;
        });

        appBuilder.Services.Configure<EventBusOptions>(appBuilder.Configuration.GetSection(subscribeSectionName ?? SectionName));

        appBuilder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
        appBuilder.Services.AddSingleton<IHostedService>(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(appBuilder.Services);
    }
}