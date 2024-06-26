using MyTimedTask;

namespace WebApplication1.TimedTasks;

public class CustomTimedTask2 : ITimedTask
{

    private readonly TestCustomTimedTaskService _testCustomTimedTaskService;

    public CustomTimedTask2(TestCustomTimedTaskService testCustomTimedTaskService)
    {
        _testCustomTimedTaskService = testCustomTimedTaskService;
    }
    public Task Execute(TimedTaskDataMap timedTaskDataMap)
    {
        var t = _testCustomTimedTaskService.TestCustomTimedTask();
        Console.WriteLine($"Hello, {timedTaskDataMap.Get<string>("Name")} called TestCustomTimedTaskService.TestCustomTimedTask, result = {t}");
        return Task.CompletedTask;
    }
}
