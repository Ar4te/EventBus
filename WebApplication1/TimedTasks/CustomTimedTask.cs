using MyTimedTask;

namespace WebApplication1.TimedTasks;

public class CustomTimedTask : ITimedTask
{
    private readonly TestCustomTimedTaskService _testCustomTimedTaskService;

    public CustomTimedTask(TestCustomTimedTaskService testCustomTimedTaskService)
    {
        _testCustomTimedTaskService = testCustomTimedTaskService;
    }
    public Task Execute(TimedTaskDataMap timedTaskDataMap)
    {
        Console.WriteLine("Hello");
        var t = _testCustomTimedTaskService.TestCustomTimedTask();
        Console.WriteLine($"Called TestCustomTimedTaskService.TestCustomTimedTask, result = {t}");
        return Task.CompletedTask;
    }
}
