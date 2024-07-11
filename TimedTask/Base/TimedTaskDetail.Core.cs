using TimedTask.Base;
using TimedTask.Lock;

namespace TimedTask;

public sealed partial class TimedTaskDetail : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private PeriodicTimer _periodicTimer;
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

    private TimedTaskDetail(string name, TimeSpan interval, Func<Task> taskFunc, TimedTaskDataMap dataMap, bool startNow = false, int startAt = 0, int repeats = -1) : this()
    {
        Name = name;
        Interval = interval;
        TaskFunc = taskFunc;
        TimedTaskDataMap = dataMap;
        StartNow = startNow;
        Repeats = repeats;
        if (startAt < 0) throw new InvalidOperationException(nameof(startAt) + "must bigger than zero");
        StartAt = TimeSpan.FromSeconds(startAt);
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

    public void Start()
    {
        if (_isRunning)
        {
            throw new InvalidOperationException($"Task [{Name}] is already running.");
        }

        _isRunning = true;

        _periodicTimer ??= new PeriodicTimer(Interval);

        Task.Run(async () =>
        {
            try
            {
                if (StartAt > TimeSpan.Zero)
                {
                    await Task.Delay(StartAt, _cts.Token);
                }

                while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
                {
                    using (var taskLock = await TimedTaskLockManager.GetLockAsync(Id))
                    {
                        try
                        {
                            if (_isPause) continue;
                            await TaskFunc();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error in task [{Name}]: {e}");
                        }
                        finally
                        {
                            _ranCount++;
                        }
                        if (_ranCount == Repeats) break;
                    }
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
        }, _cts.Token);
    }

    public void Stop()
    {
        if (_isRunning)
            _isRunning = false;
        _cts.Cancel();
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