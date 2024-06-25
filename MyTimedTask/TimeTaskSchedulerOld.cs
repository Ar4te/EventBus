using System.Collections.Concurrent;

namespace MyTimedTask;

public class TimeTaskSchedulerOld
{
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _tasks = new();
    public async Task AddTask<T>(string name, TimeSpan interval, TimedTaskDataMap dataMap,
        bool startNow = false, int startAt = default) where T : ITimedTask
    {
        var _task = Activator.CreateInstance(typeof(T)) as ITimedTask;
        var task = new TimedTaskDetail(name, interval, () => _task.Execute(dataMap), dataMap, startNow, startAt);

        if (!_tasks.TryAdd(name, task))
        {
            throw new InvalidOperationException($"Task with name {task.Name} already exists.");
        }

        Console.WriteLine($"Task with name {task.Name} was added.");
        if (task.StartNow || task.StartAt == TimeSpan.Zero)
        {
            task.Start();
            Console.WriteLine($"Task with name {task.Name} was ran now.");
        }
        else if (task.StartAt > TimeSpan.Zero)
        {
            Console.WriteLine($"Task with name {task.Name} was ran after {task.StartAt.Seconds}s.");
            await Task.Delay(task.StartAt).ContinueWith(_ => task.Start());
        }
    }

    public void RemoveTask(string taskName)
    {
        if (!_tasks.TryRemove(taskName, out var value))
        {
            throw new InvalidOperationException($"Task with name {taskName} does not exist.");
        }

        value.Stop();
    }

    public void StartAll()
    {
        foreach (var task in _tasks.Values)
        {
            task.Start();
        }
    }

    public void StartTimedTask(string key)
    {
        foreach (var task in _tasks.Values)
        {
            task.Start();
        }
    }

    public void StopAll()
    {
        foreach (var task in _tasks.Values)
        {
            task.Stop();
        }
    }

    public void StopTaskByName(string taskName)
    {
        if (!_tasks.TryGetValue(taskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {taskName} does not exist.");
        }

        task.Stop();
    }
}