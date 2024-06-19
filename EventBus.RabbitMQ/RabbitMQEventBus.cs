using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ;

public sealed class RabbitMQEventBus : IEventBus, IDisposable, IHostedService
{
    private const string ExchangeName = "eshop_event_bus";

    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;

    private readonly EventBusSubscriptionInfo _subscriptionInfo;
    private readonly ResiliencePipeline _pipeline;
    private IConnection _rabbitMQConnection;
    private IModel _consumerChannel;

    public RabbitMQEventBus(
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider,
        IOptions<EventBusOptions> options,
        IOptions<EventBusSubscriptionInfo> subscriptionOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _queueName = options.Value.SubscriptionClientName;
        _subscriptionInfo = subscriptionOptions.Value;
        _pipeline = CreateResiliencePipeline(options.Value.RetryCount);
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    public Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, routingKey);
        }

        using var channel = _rabbitMQConnection?.CreateModel() ?? throw new InvalidOperationException("RabbitMQ connection is not open");

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
        }

        channel.ExchangeDeclare(exchange: ExchangeName, type: "direct");

        var body = SerializeMessage(@event);

        return _pipeline.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();

            properties.DeliveryMode = 2;

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            }

            try
            {
                channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey, mandatory: true, basicProperties: properties, body: body);
            }
            catch (Exception)
            {

            }
            return Task.CompletedTask;
        });
    }

    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew(() =>
        {
            try
            {
                _logger.LogInformation("Starting RabbitMQ connection on a background thread");

                _rabbitMQConnection = _serviceProvider.GetRequiredService<IConnection>();
                if (!_rabbitMQConnection.IsOpen)
                {
                    return;
                }

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Creating RabbitMQ consumer channel");
                }

                _consumerChannel = _rabbitMQConnection.CreateModel();

                _consumerChannel.CallbackException += (sender, ea) =>
                {
                    _logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                };

                _consumerChannel.ExchangeDeclare(exchange: ExchangeName, type: "direct");

                _consumerChannel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Starting RabbitMQ basic consume");
                }

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += OnMessageReceived;

                _consumerChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    _consumerChannel.QueueBind(queue: _queueName, exchange: ExchangeName, routingKey: eventName);
                }

                cancellationToken.Register(() =>
                {
                    _logger.LogInformation("Cancellation requested. Closing RabbitMQ connection.");
                    _consumerChannel.Close();
                    _rabbitMQConnection.Close();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        },
        cancellationToken,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;

        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        await ProcessEvent(eventName, message).ConfigureAwait(false);

        _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);

        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType);
        foreach (var handler in handlers)
        {
            await handler.Handle(integrationEvent);
        }
    }

    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return (JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent)!;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        static TimeSpan? GenerateDelay(int attemptNumber)
        {
            return TimeSpan.FromSeconds(attemptNumber);
        }
    }
}
