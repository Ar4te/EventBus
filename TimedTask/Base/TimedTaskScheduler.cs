﻿global using static Infrastructure.OperateResultExtension;
using System.Collections.Concurrent;
using System.Text;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TimedTask.Extensions;

namespace TimedTask.Base;

#region core
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
            .For<T>(async () => await _task.Execute(dataMap))
            .WithRepeats()
            .UseTaskDataMap(dataMap)
            .StartNow(startNow)
            .StartAt(startAt);

        AddTask(timedTaskDetail);
    }

    public void AddTask(TimedTaskDetail timedTaskDetail)
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

        return errMsg.Length > 0 ? Fail(errMsg.ToString()) : Success();
    }

    public OperateResult StartTimedTask(string taskName)
    {
        try
        {
            if (_tasks.TryGetValue(taskName, out var timedTaskDetail))
            {
                return StartTask(timedTaskDetail);
            }
            return Fail($"Task with name {taskName} does not exist.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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

        return errMsg.Length > 0 ? Fail(errMsg.ToString()) : Success();
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
            return Fail($"Task with name {timedTaskName} does not run.");
        }

        task.Pause();
        _pausedTasks.AddOrUpdate(timedTaskName, task, (_, exist) => task);
        return Success($"{timedTaskName} was paused successful");
    }

    public OperateResult ResumeTask(string timedTaskName)
    {
        if (!_pausedTasks.TryRemove(timedTaskName, out var task))
        {
            return Fail($"Task with name {timedTaskName} does not pause.");
        }

        task.Resume();
        _runningTasks.AddOrUpdate(timedTaskName, task, (_, exist) => task);
        return Success($"{timedTaskName} was resumed successful");
    }

    public OperateResult PauseTasks(string groupName)
    {
        if (_timedTaskGroupInfos.ContainsKey(groupName) && _timedTaskGroupInfos.TryGetValue(groupName, out var bag) && bag.Count != 0)
        {
            List<string> errMsg = new();
            Parallel.ForEach(bag.Values, timedTaskName =>
            {
                if (PauseTask(timedTaskName) is { IsSuccess: false })
                {
                    errMsg.Add(timedTaskName);
                }
            });

            if (errMsg?.Count > 0)
            {
                return Fail(string.Join(';', errMsg));
            }

            return Success();
        }

        return Fail($"{groupName} does not exist");
    }

    public OperateResult ResumeTasks(string groupName)
    {
        if (_timedTaskGroupInfos.ContainsKey(groupName) && _timedTaskGroupInfos.TryGetValue(groupName, out var bag) && bag.Count != 0)
        {
            StringBuilder errMsg = new();
            foreach (var item in bag.Values.Select(timedTaskName => ResumeTask(timedTaskName)).Where(t => t is { IsSuccess: false }))
            {
                errMsg.AppendLine(item.ErrorMessage);
            }

            if (errMsg.Length > 0)
            {
                return Fail(errMsg.ToString());
            }

            return Success();
        }

        return Fail($"{groupName} does not exist");
    }
}
#endregion

#region PrivateMethod
public sealed partial class TimedTaskScheduler
{
    private OperateResult StopTask(TimedTaskDetail timedTask)
    {
        try
        {
            if (_runningTasks.TryRemove(new(timedTask.Name, timedTask)))
            {
                timedTask.Stop();
                _timedTaskGroupInfos[timedTask.Group].Remove(timedTask.Name);
                return Success();
            }
            return Fail($"停止[{timedTask.Name}]发生异常：" + "从运行列表中删除任务失败");
        }
        catch (Exception ex)
        {
            return Fail($"停止[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }

    private OperateResult StartTask(TimedTaskDetail timedTask)
    {
        try
        {
            _ = Task.Run(async () => await timedTask.Start());
            _runningTasks.AddOrUpdate(timedTask.Name, timedTask, (_, old) => timedTask);
            return Success();
        }
        catch (Exception ex)
        {
            return Fail($"启动[{timedTask.Name}]发生异常：" + ex.Message);
        }
    }
}
#endregion