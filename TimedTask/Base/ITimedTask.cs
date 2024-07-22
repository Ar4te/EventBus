namespace TimedTask.Base;

public interface ITimedTask
{
    Task Execute(TimedTaskDataMap timedTaskDataMap);
}