using System.Collections.Concurrent;
using System.Text;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TimedTask.Extensions;

namespace TimedTask.Base;

public sealed partial class TimedTaskScheduler
{
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _tasks = new();
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _runningTasks = new();
    private readonly ConcurrentDictionary<string, TimedTaskDetail> _pausedTasks = new();
    private readonly ConcurrentDictionary<string, ThreadSafeBag<string>> _timedTaskGroupInfos = new();
    private readonly IServiceProvider _serviceProvider;

    public TimedTaskScheduler(IServiceScopeFactory serviceScopeFactory)
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

        _timedTaskGroupInfos.AddOrUpdate(
            timedTaskDetail.Group,
            _ =>
            {
                var queue = new ThreadSafeBag<string>();
                queue.Add(timedTaskDetail.Name);
                return queue;
            },
            (_, old) =>
            {
                old.Add(timedTaskDetail.Name);
                return old;
            }
        );
    }

    public OperateResult StartAll()
    {
        var errMsg = new StringBuilder();
        foreach (var task in _tasks.Values)
        {
            if (StartTask(task) is { IsSuccess: false } result)
            {
                errMsg.Append(result.ErrorMessage);
            }
        }

        return errMsg.Length > 0 ?
            OperateResultExtension.Fail(errMsg.ToString()) :
            OperateResultExtension.Success();
    }

    public OperateResult StartTimedTask(string taskName)
    {
        try
        {
            if (_tasks.TryGetValue(taskName, out var timedTaskDetail))
            {
                return StartTask(timedTaskDetail);
            }
            return OperateResultExtension.Fail($"Task with name {taskName} does not exist.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public OperateResult StopAll()
    {
        var errMsg = new StringBuilder();
        foreach (var task in _tasks.Values)
        {
            if (StopTask(task) is OperateResult result && !result.IsSuccess)
            {
                errMsg.Append(result.ErrorMessage);
            }
        }

        return errMsg.Length > 0 ?
            OperateResultExtension.Fail(errMsg.ToString()) :
            OperateResultExtension.Success();
    }

    public OperateResult StopTask(string taskName)
    {
        if (!_tasks.TryGetValue(taskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {taskName} does not exist.");
        }

        return StopTask(task);
    }

    public int GetTaskRanCount(string timedTaskName)
    {
        if (!_tasks.TryGetValue(timedTaskName, out var task))
        {
            throw new InvalidOperationException($"Task with name {timedTaskName} does not exist.");
        }

        return task.GetRanCount();
    }

    public OperateResult PauseTask(string timedTaskName)
    {
        if (!_runningTasks.TryRemove(timedTaskName, out var task))
        {
            return OperateResultExtension.Fail($"Task with name {timedTaskName} does not run.");
        }

        task.Pause();
        _pausedTasks.AddOrUpdate(timedTaskName, task, (_, exist) => task);
        return OperateResultExtension.Success($"{timedTaskName} was paused successful");
    }

    public OperateResult ResumeTask(string timedTaskName)
    {
        if (!_pausedTasks.TryRemove(timedTaskName, out var task))
        {
            return OperateResultExtension.Fail($"Task with name {timedTaskName} does not pause.");
        }

        task.Resume();
        _runningTasks.AddOrUpdate(timedTaskName, task, (_, exist) => task);
        return OperateResultExtension.Success($"{timedTaskName} was resumed successful");
    }

    public OperateResult PauseTasks(string groupName)
    {
        if (_timedTaskGroupInfos.ContainsKey(groupName) && _timedTaskGroupInfos.TryGetValue(groupName, out var bag) && bag.Count != 0)
        {
            StringBuilder errMsg = new();
            foreach (var timedTaskName in bag.Values)
            {
                if (PauseTask(timedTaskName) is { IsSuccess: false } result)
                {
                    errMsg.AppendLine(timedTaskName);
                }
            }

            if (errMsg.Length > 0)
            {
                return OperateResultExtension.Fail(errMsg.ToString());
            }

            return OperateResultExtension.Success();
        }

        return OperateResultExtension.Fail($"{groupName} does not exist");
    }

    public OperateResult ResumeTasks(string groupName)
    {
        if (_timedTaskGroupInfos.ContainsKey(groupName) && _timedTaskGroupInfos.TryGetValue(groupName, out var bag) && bag.Count != 0)
        {
            StringBuilder errMsg = new();
            foreach (var timedTaskName in bag.Values)
            {
                if (ResumeTask(timedTaskName) is { IsSuccess: false } result)
                {
                    errMsg.AppendLine(timedTaskName);
                }
            }

            if (errMsg.Length > 0)
            {
                return OperateResultExtension.Fail(errMsg.ToString());
            }

            return OperateResultExtension.Success();
        }

        return OperateResultExtension.Fail($"{groupName} does not exist");
    }
}

public class OperateResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public OperateResult()
    {
        IsSuccess = false;
    }

    public OperateResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}

public class OperateResult<T> : OperateResult
{
    public T? Data { get; set; }

    public OperateResult() : base()
    {
    }

    public OperateResult(bool isSuccess, string errorMessage, T? data = default) : base(isSuccess, errorMessage)
    {
        Data = data;
    }
}

public static class OperateResultExtension
{
    public static OperateResult Success(string errorMessage = "")
    {
        return new OperateResult(true, errorMessage);
    }

    public static OperateResult Fail(string errorMessage = "")
    {
        return new OperateResult(false, errorMessage);
    }

    public static OperateResult<T> Success<T>(string errorMessage = "", T? data = default)
    {
        return new OperateResult<T>(true, errorMessage, data);
    }

    public static OperateResult<T> Fail<T>(string errorMessage = "", T? data = default)
    {
        return new OperateResult<T>(false, errorMessage, data);
    }
}