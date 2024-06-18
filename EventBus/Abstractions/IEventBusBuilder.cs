using System.Runtime.ConstrainedExecution;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Abstractions;

public interface IEventBusBuilder
{
    public IServiceCollection Services { get; }
}


public class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
{
    public IServiceCollection Services => services;
}
