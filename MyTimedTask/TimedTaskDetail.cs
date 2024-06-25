namespace MyTimedTask;

public class TimedTaskDetail
{
    private readonly CancellationTokenSource _cts;
    private readonly Func<Task> _taskFunc;

    public TimedTaskDetail(string name, TimeSpan interval, Func<Task> taskFunc, TimedTaskDataMap dataMap, bool startNow = false, int startAt = 0)
    {
        Name = name;
        Interval = interval;
        _taskFunc = taskFunc;
        TimedTaskDataMap = dataMap;
        StartNow = startNow;
        if (startAt < 0) throw new InvalidOperationException(nameof(startAt) + "must bigger than zero");
        StartAt = TimeSpan.FromSeconds(startAt);
        _cts = new CancellationTokenSource();
    }

    public string Name { get; }
    public TimeSpan Interval { get; }
    public TimedTaskDataMap TimedTaskDataMap { get; }
    public bool StartNow { get; }
    public TimeSpan StartAt { get; }


    public void Start()
    {
        Task.Factory.StartNew(async () =>
        {
            if (StartAt > TimeSpan.Zero)
            {
                await Task.Delay(StartAt, _cts.Token);
            }

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await _taskFunc();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await Task.Delay(Interval, _cts.Token);
            }
        }, _cts.Token);
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}