using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace MyTimedTask;

public class TimeTaskScheduler
{
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _tasks = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IServiceProvider _serviceProvider;
    public TimeTaskScheduler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
    }

    public void AddTask<T>(string name, TimeSpan interval, TimedTaskDataMap dataMap,
        bool startNow = false, int startAt = default) where T : ITimedTask
    {
        //using var scope = _serviceScopeFactory.CreateScope();
        //var serviceProvider = scope.ServiceProvider;
        var _task = _serviceProvider.GetService(typeof(T)) as ITimedTask ?? throw new ArgumentNullException(typeof(T).Name);
        dataMap.Put("Name", name);

        var task = TimedTaskDetail.Build()
            .WithName(name)
            .WithInterval(interval)
            .For<T>(() => _task.Execute(dataMap))
            .WithRepeats()
            .UseTaskDataMap(dataMap)
            .StartNow(startNow)
            .StartAt(startAt);

        //var task = new TimedTaskDetail(name, interval, , dataMap, startNow, startAt);

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
            Task.Run(async () =>
            {
                await Task.Delay(task.StartAt);
                task.Start();
            });
        }
    }

    public void AddTask<T>(TimedTaskDetail timedTaskDetail) where T : ITimedTask
    {
        if (!_tasks.TryAdd(timedTaskDetail.Name, timedTaskDetail))
        {
            throw new InvalidOperationException($"Task with name {timedTaskDetail.Name} already exists.");
        }

        Console.WriteLine($"Task with name {timedTaskDetail.Name} was added.");
        if (timedTaskDetail.StartNow || timedTaskDetail.StartAt == TimeSpan.Zero)
        {
            timedTaskDetail.Start();
            Console.WriteLine($"Task with name {timedTaskDetail.Name} was ran now.");
        }
        else if (timedTaskDetail.StartAt > TimeSpan.Zero)
        {
            Console.WriteLine($"Task with name {timedTaskDetail.Name} was ran after {timedTaskDetail.StartAt.Seconds}s.");
            Task.Run(async () =>
            {
                await Task.Delay(timedTaskDetail.StartAt);
                timedTaskDetail.Start();
            });
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