using EventBus.Abstractions;
using MediatR;
using WebApplication1.Application.Queries;

namespace WebApplication1.Apis;

public class TestServices : ITestServices
{
    private readonly IEventBus _eventBus;

    public TestServices(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public IEventBus EventBus => _eventBus;

    public async Task SSS()
    {
        await Task.CompletedTask;
    }
}

public interface ITestServices
{
    public IEventBus EventBus { get; }
    Task SSS();
}