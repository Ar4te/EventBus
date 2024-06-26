namespace MyTimedTask;

public class TimedTaskDetail
{
    private readonly CancellationTokenSource _cts;
    private readonly Func<Task> _taskFunc;
    #region Timer
    //private readonly Timer _timer;
    #endregion
    private readonly PeriodicTimer _periodicTimer;

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
        #region Timer
        //_timer = new Timer(async _ => await ExecuteAsync(), null, Timeout.Infinite, Timeout.Infinite);
        #endregion
        _periodicTimer = new PeriodicTimer(Interval);
    }

    public string Name { get; }
    public TimeSpan Interval { get; }
    public TimedTaskDataMap TimedTaskDataMap { get; }
    public bool StartNow { get; }
    public TimeSpan StartAt { get; }


    public void Start()
    {
        Task.Run(async () =>
        {
            if (StartAt > TimeSpan.Zero)
            {
                await Task.Delay(StartAt, _cts.Token);
            }

            #region while
            while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    await _taskFunc();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //await Task.Delay(Interval, _cts.Token);
            }
            #endregion

            #region Timer
            //_timer.Change(TimeSpan.Zero, Interval);
            #endregion

        }, _cts.Token);
    }

    public void Stop()
    {
        _cts.Cancel();
        //_timer.Change(Timeout.Infinite, Timeout.Infinite);
        _periodicTimer.Dispose();
    }

    #region Timer
    //private async Task ExecuteAsync()
    //{
    //    try
    //    {
    //        await _taskFunc();
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //    finally
    //    {
    //        if (!_cts.IsCancellationRequested)
    //        {
    //            _timer.Change(Interval, Timeout.InfiniteTimeSpan);
    //        }
    //    }
    //}
    #endregion
}