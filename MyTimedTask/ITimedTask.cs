namespace MyTimedTask;

public interface ITimedTask
{
    Task Execute(TimedTaskDataMap timedTaskDataMap);
}