using TimedTask.Base;

namespace TimedTask;

public sealed partial class TimedTaskDetail
{
    public static TimedTaskDetail Build() => new();

    internal void SetInterval(TimeSpan interval)
    {
        Interval = interval;
        InitialPeriodicTimer();
    }

    internal void SetRepeats(int repeats) => Repeats = repeats;

    internal void SetTimedTaskName(string timedTaskName) => Name = timedTaskName;

    internal void SetTimedTaskDataMap(string key, object value)
    {
        TimedTaskDataMap ??= new TimedTaskDataMap();
        TimedTaskDataMap.Put(key, value);
    }

    internal void UseTimedTaskDataMap(TimedTaskDataMap timedTaskDataMap) => TimedTaskDataMap = timedTaskDataMap;

    internal void SetStartNow(bool startNow) => StartNow = startNow;

    internal void SetExecuteFunc(Func<Task> func) => TaskFunc = func;

    internal void SetStartAt(int startAt)
    {
        if (startAt < 0) throw new InvalidOperationException(nameof(startAt) + "must bigger than zero");
        StartAt = TimeSpan.FromSeconds(startAt);
    }

    internal int GetRanCount() => _ranCount;

    internal void Pause()
    {
        lock (_timedTaskDetailLock)
        {
            if (!_isPause)
            {
                _isPause = true;
            }
        }
    }

    internal void Resume()
    {
        lock (_timedTaskDetailLock)
        {
            if (_isPause)
            {
                _isPause = false;
            }
        }
    }

    internal void SetGroup(string groupName) => Group = groupName;
}