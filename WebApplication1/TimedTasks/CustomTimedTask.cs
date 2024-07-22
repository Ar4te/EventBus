using TimedTask.Base;

namespace WebApplication1.TimedTasks;

public class CustomTimedTask : ITimedTask
{
    private readonly TestCustomTimedTaskService _testCustomTimedTaskService;

    public CustomTimedTask(TestCustomTimedTaskService testCustomTimedTaskService)
    {
        _testCustomTimedTaskService = testCustomTimedTaskService;
    }

    public async Task Execute(TimedTaskDataMap timedTaskDataMap)
    {
        var t = await _testCustomTimedTaskService.TestCustomTimedTask();
        Console.WriteLine($"Hello, {timedTaskDataMap.Get<string>("Name")}  called TestCustomTimedTaskService.TestCustomTimedTask, result = {t}");
        //return Task.CompletedTask;
    }
}