using TimedTask;
using TimedTask.Base;

namespace MyTimedTask;

public partial class TimedTaskDetail
{
    public static TimedTaskDetail Build() => new();

    internal void SetInterval(TimeSpan interval)
    {
        Interval = interval;
        _periodicTimer = new PeriodicTimer(interval);
    }

    public void SetRepeats(int repeats) => Repeats = repeats;

    public void SetTimedTaskName(string timedTaskName) => Name = timedTaskName;

    public void SetTimedTaskDataMap(string key, object value)
    {
        if (TimedTaskDataMap is null)
        {
            TimedTaskDataMap = new TimedTaskDataMap();
            TimedTaskDataMap.Put(key, value);
        }
        else
        {
            TimedTaskDataMap.Put(key, value);
        }
    }

    public void UseTimedTaskDataMap(TimedTaskDataMap timedTaskDataMap) => TimedTaskDataMap = timedTaskDataMap;

    public void SetStartNow(bool startNow) => StartNow = startNow;

    internal void SetExecuteFunc(Func<Task> func)
    {
        TaskFunc = func;
    }

    internal void SetStartAt(int startAt)
    {
        if (startAt < 0) throw new InvalidOperationException(nameof(startAt) + "must bigger than zero");
        StartAt = TimeSpan.FromSeconds(startAt);
    }
}

public static class TimedTaskDetailExtension
{
    public static TimedTaskDetail WithName(this TimedTaskDetail @this, string timedTaskName)
    {
        @this.SetTimedTaskName(timedTaskName);
        return @this;
    }
    public static TimedTaskDetail WithInterval(this TimedTaskDetail @this, TimeSpan interval)
    {
        @this.SetInterval(interval);
        return @this;
    }
    /// <summary>
    /// 设置循环次数
    /// 0和-1 为无限次数
    /// </summary>
    /// <param name="this"></param>
    /// <param name="repeats"></param>
    /// <returns></returns>
    public static TimedTaskDetail WithRepeats(this TimedTaskDetail @this, int repeats = -1)
    {
        @this.SetRepeats(repeats);
        return @this;
    }
    public static TimedTaskDetail UseTaskDataMap(this TimedTaskDetail @this, TimedTaskDataMap timedTaskDataMap)
    {
        @this.UseTimedTaskDataMap(timedTaskDataMap);
        return @this;
    }
    public static TimedTaskDetail SetTaskDataMap(this TimedTaskDetail @this, string key, object value)
    {
        @this.SetTimedTaskDataMap(key, value);
        return @this;
    }
    public static TimedTaskDetail StartNow(this TimedTaskDetail @this, bool startNow = true)
    {
        @this.SetStartNow(startNow);
        return @this;
    }
    /// <summary>
    /// 启动延时（分钟）
    /// </summary>
    /// <param name="this"></param>
    /// <param name="startAt"></param>
    /// <returns></returns>
    public static TimedTaskDetail StartAt(this TimedTaskDetail @this, int startAt)
    {
        @this.SetStartAt(startAt);
        return @this;
    }
    public static TimedTaskDetail For<T>(this TimedTaskDetail @this, Func<Task> func) where T : ITimedTask
    {
        @this.SetExecuteFunc(func);
        return @this;
    }
}