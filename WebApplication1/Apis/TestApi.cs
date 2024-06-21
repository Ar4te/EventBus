using EventBus.Abstractions;
using EventBus.EventLog.EFCore.Services;
using EventBus.Events;
using Infrastructure;

namespace WebApplication1.Apis;

public static class TestApi
{
    public static RouteGroupBuilder MapTestApiV1(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var api = endpointRouteBuilder.MapGroup("api/tests");

        api.MapGet("/", TestGet);
        return api;
    }

    private static async Task TestGet([AsParameters] TestServices testServices)
    {
        var @event = new Tes32tIntegrationEvent("TestInfo");
        await testServices.EventBus.PublishAsync(@event);
        await Task.CompletedTask;
    }
}

public record Tes32tIntegrationEvent : IntegrationEvent
{
    public string Info { get; set; }

    public Tes32tIntegrationEvent() : base()
    {

    }

    public Tes32tIntegrationEvent(string info) : base()
    {
        Info = info;
    }
}

public class TestEventHandler : IIntegrationEventHandler<Tes32tIntegrationEvent>
{
    private readonly ILogger<TestEventHandler> _logger;

    public TestEventHandler(ILogger<TestEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(Tes32tIntegrationEvent @event)
    {
        _logger.LogInformation("TestEventHandler Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        return Task.CompletedTask;
    }
}

public class TestEventHandler2 : IIntegrationEventHandler<Tes32tIntegrationEvent>
{
    private readonly ILogger<TestEventHandler2> _logger;
    private readonly IIntegrationEventLogService _integrationEventLogService;

    public TestEventHandler2(ILogger<TestEventHandler2> logger, IIntegrationEventLogService integrationEventLogService)
    {
        _logger = logger;
        _integrationEventLogService = integrationEventLogService;
    }

    public async Task Handle(Tes32tIntegrationEvent @event)
    {
        //Test3.Test();
        Test3.Test2();
        //await _integrationEventLogService.MarkEventAsConsumedAsync(@event.Id);
        _logger.LogInformation("TestEventHandler2 Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
    }
}

public class TestClass
{
    public TestClass()
    {

    }
    public TestClass(Guid id, string info, List<string> datas)
    {
        Id = id;
        Info = info;
        Datas = datas;
    }
    public Guid Id { get; set; }
    public string Info { get; set; }
    public List<string> Datas { get; set; }
}

public static class Test3
{
    //public static void Test()
    //{
    //    Console.WriteLine("Test====");
    //    List<string> datas = ["1", "2", "3"];
    //    var t1 = new TestClass(Guid.NewGuid(), "123", datas);
    //    Console.WriteLine(JsonSerializer.Serialize(t1));
    //    var t2 = TransExp<TestClass, TestClass>.Trans(t1);
    //    Console.WriteLine(JsonSerializer.Serialize(t2));
    //    datas[0] = "4";
    //    Console.WriteLine(JsonSerializer.Serialize(t1));
    //    Console.WriteLine(JsonSerializer.Serialize(t2));
    //    Console.WriteLine("Test====");
    //}

    public static void Test2()
    {
        Console.WriteLine("Test2====");
        List<string> datas = ["1", "2", "3"];
        var t3 = new TestClass(Guid.NewGuid(), "123", datas);
        Console.WriteLine(t3);
        //Console.WriteLine(JsonSerializer.Serialize(t3));
        var t4 = t3.DeepCopy();
        //var t4 = JsonSerializer.Deserialize<TestClass>(JsonSerializer.Serialize(t3));
        Console.WriteLine(t4);
        datas[0] = "4";
        Console.WriteLine(t3);
        Console.WriteLine(t4);
        Console.WriteLine("Test2====");
    }
}