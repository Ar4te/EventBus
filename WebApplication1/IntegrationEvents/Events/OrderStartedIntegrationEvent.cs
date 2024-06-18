using EventBus.Events;

namespace WebApplication1.IntegrationEvents.Events;

public record OrderStartedIntegrationEvent(string UserId) : IntegrationEvent;
