using TimedTask.Base;
using TimedTask.Lock;

namespace TimedTask;

#region core
public sealed partial class TimedTaskDetail : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private PeriodicTimer? _periodicTimer;
    private volatile bool _isRunning = false;
    private int _ranCount = 0;
    private bool _isPause = false;
    private bool _disposedValue;
    private readonly object _timedTaskDetailLock = new();

    private TimedTaskDetail()
    {
        Id = Guid.NewGuid();
        _cts = new CancellationTokenSource();
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public TimeSpan Interval { get; private set; }
    public TimedTaskDataMap TimedTaskDataMap { get; private set; }
    public bool StartNow { get; private set; }
    public int Repeats { get; private set; }
    public TimeSpan StartAt { get; private set; }
    public Func<Task> TaskFunc { get; private set; }
    public string Group { get; private set; } = "Default";

    public async Task Start()
    {
        lock (_timedTaskDetailLock)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException($"Task [{Name}] is already running.");
            }
            _isRunning = true;
        }

        _periodicTimer ??= new PeriodicTimer(Interval);

        await Task.Run(async () =>
          {
              try
              {
                  if (StartAt > TimeSpan.Zero)
                  {
                      await Task.Delay(StartAt, _cts.Token);
                  }

                  while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
                  {
                      if (_isPause) continue;
                      var taskLock = await TimedTaskLockManager.GetLockAsync(Id);

                      try
                      {
                          await TaskFunc();
                      }
                      catch (Exception e)
                      {
                          Console.WriteLine($"Error in task [{Name}]: {e}");
                      }
                      finally
                      {
                          taskLock.Release();
                          _ranCount++;
                      }
                      if (_ranCount == Repeats) break;
                  }
              }
              catch (OperationCanceledException)
              {
                  Console.WriteLine($"Task [{Name}] was canceled.");
              }
              catch (Exception e)
              {
                  Console.WriteLine($"Unexpected error in task [{Name}]: {e}");
              }
              finally
              {
                  Stop();
              }
          },
          _cts.Token);
    }

    public void Stop()
    {
        lock (_timedTaskDetailLock)
        {
            if (_isRunning)
                _isRunning = false;
            _cts.Cancel();
        }
    }

    private void InitialPeriodicTimer()
    {
        _periodicTimer?.Dispose();
        _periodicTimer = new PeriodicTimer(Interval);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts.Cancel();
                _periodicTimer?.Dispose();
                _periodicTimer = null;
                _cts.Dispose();
            }

            _disposedValue = true;
        }
    }

    ~TimedTaskDetail()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#endregion

#region builder
public sealed partial class TimedTaskDetail
{
    public static TimedTaskDetail Build() => new();

    internal void SetInterval(TimeSpan interval)
    {
        Interval = interval;
        InitialPeriodicTimer();
    }

    internal void SetRepeats(int repeats)
    {
        if (repeats < -1)
        {
            repeats = -1;
        }
        Repeats = repeats;
    }

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
#endregion