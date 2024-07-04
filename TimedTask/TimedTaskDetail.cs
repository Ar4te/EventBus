using TimedTask;

namespace MyTimedTask;

public partial class TimedTaskDetail
{
    private readonly CancellationTokenSource _cts;
    private PeriodicTimer _periodicTimer;
    private SemaphoreSlim _semaphoreSlim;
    private bool _isRunning = false;
    private int _ranCount = 0;
    private bool _isPause = false;
    private readonly object _timedTaskDetailLock = new();

    private TimedTaskDetail()
    {
        _cts = new CancellationTokenSource();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
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

    public string Name { get; private set; }
    public TimeSpan Interval { get; private set; }
    public TimedTaskDataMap TimedTaskDataMap { get; private set; }
    public bool StartNow { get; private set; }
    public int Repeats { get; private set; }
    public TimeSpan StartAt { get; private set; }
    public Func<Task> TaskFunc { get; private set; }

    public void Start()
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
                    try
                    {
                        await _semaphoreSlim.WaitAsync(_cts.Token);
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
                        _semaphoreSlim.Release();
                    }
                    if (_ranCount == Repeats) break;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Task [{Name}] was canceled.");
            }
            finally
            {
                Stop();
            }
            Stop();
        }, _cts.Token);
    }

    public void Stop()
    {
        lock (_timedTaskDetailLock)
        {
            if (!_isRunning)
            {
                return;
            }
            _isRunning = false;
        }
        _cts.Cancel();
        _periodicTimer?.Dispose();
        _semaphoreSlim?.Dispose();
    }

    private void InitialPeriodicTimer()
    {
        _periodicTimer?.Dispose();
        _periodicTimer = new PeriodicTimer(Interval);
    }
}