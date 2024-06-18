using EventBus.Events;

namespace EventBus.Abstractions;

public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);

    Task IIntegrationEventHandler.Handle(IntegrationEvent @event)
    {
        if (@event is TIntegrationEvent typedEvent)
        {
            return Handle(typedEvent);
        }
        else
        {
            throw new ArgumentException($"Event is not of type {typeof(TIntegrationEvent).Name}", nameof(@event));
        }
    }
}

public interface IIntegrationEventHandler
{
    Task Handle(IntegrationEvent @event);
}
