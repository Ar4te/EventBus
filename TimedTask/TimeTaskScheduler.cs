using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using MyTimedTask;
using TimedTask.Base;

namespace TimedTask;

public class TimeTaskScheduler
{
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _tasks = new();
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _pausedTasks = new();
    private readonly IServiceProvider _serviceProvider;
    public TimeTaskScheduler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
    }

    public void AddTask<T>(string name, TimeSpan interval, TimedTaskDataMap dataMap,
        bool startNow = false, int startAt = default) where T : ITimedTask
    {
        var _task = _serviceProvider.GetService(typeof(T)) as ITimedTask ?? throw new ArgumentNullException(typeof(T).Name);
        var timedTaskDetail = TimedTaskDetail.Build()
            .WithName(name)
            .WithInterval(interval)
            .For<T>(() => _task.Execute(dataMap))
            .WithRepeats()
            .UseTaskDataMap(dataMap)
            .StartNow(startNow)
            .StartAt(startAt);

        AddTask<T>(timedTaskDetail);
    }

    public void AddTask<T>(TimedTaskDetail timedTaskDetail) where T : ITimedTask
    {
        timedTaskDetail.TimedTaskDataMap.Put("Name", timedTaskDetail.Name);
        if (!_tasks.TryAdd(timedTaskDetail.Name, timedTaskDetail))
        {
            throw new InvalidOperationException($"Task with name {timedTaskDetail.Name} already exists.");
        }

        timedTaskDetail.Start();
    }

    public (bool Success, string ErrorMessage) RemoveTask(string taskName)
    {
        if (!_tasks.TryRemove(taskName, out var value))
        {
            return (false, $"Task with name {taskName} does not exist.");
        }

        value.Stop();
        return (true, "");
    }

    public (bool Success, string ErrorMessage) StartAll()
    {
        var errMsg = new StringBuilder();
        foreach (var task in _tasks.Values)
        {
            try
            {
                task.Start();
            }
            catch (Exception ex)
            {
                errMsg.Append($"启动[{task.Name}]发生异常：" + ex.Message);
            }
        }

        return errMsg.Length > 0 ? (false, errMsg.ToString()) : (true, "");
    }

    public (bool Success, string ErrorMessage) StartTimedTask(string taskName)
    {
        try
        {
            if (_tasks.TryGetValue(taskName, out var timedTaskDetail))
            {
                timedTaskDetail.Start();
                return (true, "");
            }
            else
            {
                return (false, $"未找到{taskName}相关任务");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public (bool Success, string ErrorMessage) StopAll()
    {
        var errMsg = new StringBuilder();
        foreach (var task in _tasks.Values)
        {
            try
            {
                task.Stop();
            }
            catch (Exception ex)
            {
                errMsg.Append($"停止[{task.Name}]发生异常：" + ex.Message);
            }
        }

        return errMsg.Length > 0 ? (false, errMsg.ToString()) : (true, "");
    }

    public void StopTask(string taskName)
    {
        if (!_tasks.TryGetValue(taskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {taskName} does not exist.");
        }

        task.Stop();
    }

    public int GetTaskRanCount(string timedTaskName)
    {
        if (!_tasks.TryGetValue(timedTaskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {timedTaskName} does not exist.");
        }

        return task.GetRanCount();
    }

    public void PauseTask(string timedTaskName)
    {
        if (!_tasks.TryGetValue(timedTaskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {timedTaskName} does not exist.");
        }

        _pausedTasks.AddOrUpdate(timedTaskName, task, (_, exist) => task);
        task.Pause();
    }

    public void ResumeTask(string timedTaskName)
    {
        if (!_pausedTasks.TryRemove(timedTaskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {timedTaskName} does not pause.");
        }

        task.Resume();
    }
}