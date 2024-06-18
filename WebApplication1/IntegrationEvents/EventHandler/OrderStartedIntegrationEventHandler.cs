using EventBus.Abstractions;
using WebApplication1.IntegrationEvents.Events;
using WebApplication1.Repositories;

namespace WebApplication1.IntegrationEvents.EventHandler;

public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;
    private readonly IBasketRespository _basketRespository;

    public OrderStartedIntegrationEventHandler(ILogger<OrderStartedIntegrationEventHandler> logger, IBasketRespository basketRespository)
    {
        _logger = logger;
        _basketRespository = basketRespository;
    }

    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        _logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await _basketRespository.GetBusketAsync(@event.UserId);
    }
}
