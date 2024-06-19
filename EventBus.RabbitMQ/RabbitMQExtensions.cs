using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ;

public static class RabbitMQExtensions
{
    private const string DefaultConfigSectionClientName = "EventBus:RabbitMQ:Client";
    private const string DefaultConfigSectionFactoryName = "EventBus:RabbitMQ:Factory";

    public static void AddRabbitMQClient(
        this IHostApplicationBuilder builder,
        Action<RabbitMQClientSettings>? configureSettings = null,
        Action<ConnectionFactory>? configureConnectionFactory = null,
        string? clientSectionName = null,
        string? factorySectionName = null)
    {
        AddRabbitMQClient(builder, clientSectionName ?? DefaultConfigSectionClientName, factorySectionName ?? DefaultConfigSectionFactoryName, configureSettings, configureConnectionFactory, null);
    }

    public static void AddKeyedRabbitMQClient(
        this IHostApplicationBuilder builder,
        Action<RabbitMQClientSettings>? configureSettings = null,
        Action<ConnectionFactory>? configureConnectionFactory = null,
        string? clientSectionName = null,
        string? factorySectionName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientSectionName, nameof(clientSectionName));

        AddRabbitMQClient(builder, clientSectionName ?? DefaultConfigSectionClientName, factorySectionName ?? DefaultConfigSectionFactoryName, configureSettings, configureConnectionFactory, clientSectionName);
    }

    private static void AddRabbitMQClient(
        IHostApplicationBuilder builder,
        string clientSectionName,
        string factorySectionName,
        Action<RabbitMQClientSettings>? configureSettings,
        Action<ConnectionFactory>? configureConnectionFactory,
        object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configSection = builder.Configuration.GetSection(clientSectionName);

        var settings = new RabbitMQClientSettings();
        configSection.Bind(settings);

        configureSettings?.Invoke(settings);

        if (serviceKey is null)
        {
            builder.Services.AddSingleton(CreateConnectionFactory);
            builder.Services.AddSingleton(sp => CreateConnection(sp.GetRequiredService<IConnectionFactory>(), settings.MaxConnectRetryCount));
        }
        else
        {
            builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => CreateConnectionFactory(sp));
            builder.Services.AddKeyedSingleton(serviceKey, (sp, key) => CreateConnection(sp.GetRequiredKeyedService<IConnectionFactory>(key), settings.MaxConnectRetryCount));
        }

        IConnectionFactory CreateConnectionFactory(IServiceProvider sp)
        {
            var factory = new ConnectionFactory();

            var configurationOptionsSection = configSection.GetSection(factorySectionName);
            configurationOptionsSection?.Bind(factory);

            if (!string.IsNullOrEmpty(settings.ConnectionString))
            {
                factory.Uri = new Uri(settings.ConnectionString);
            }

            configureConnectionFactory?.Invoke(factory);

            return factory;
        }
    }

    private static IConnection CreateConnection(IConnectionFactory factory, int retryCount)
    {
        var resiliencePipelineBuilder = new ResiliencePipelineBuilder();
        if (retryCount > 0)
        {
            resiliencePipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = static args => args.Outcome is { Exception: SocketException or BrokerUnreachableException } ?
                PredicateResult.True() :
                PredicateResult.False(),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = retryCount,
                DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
            });
        }

        var resiliencePipeline = resiliencePipelineBuilder.Build();

        try
        {
            return resiliencePipeline.Execute(() => factory.CreateConnection());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create RabbitMQ connection", ex);
        }

        static TimeSpan? GenerateDelay(int attemptNumber)
        {
            return TimeSpan.FromSeconds(attemptNumber);
        }
    }
}

public sealed class RabbitMQClientSettings
{
    public string? ConnectionString { get; set; }
    public int MaxConnectRetryCount { get; set; } = 5;
}