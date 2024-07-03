using TimedTask;

namespace MyTimedTask;

public partial class TimedTaskDetail
{
    private readonly CancellationTokenSource _cts;
    #region Timer
    //private readonly Timer _timer;
    #endregion
    private PeriodicTimer _periodicTimer;

    private TimedTaskDetail()
    {
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
        #region Timer
        //_timer = new Timer(async _ => await ExecuteAsync(), null, Timeout.Infinite, Timeout.Infinite);
        #endregion
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
        Task.Run(async () =>
        {
            //int s = Environment.TickCount;
            //if (StartAt > TimeSpan.Zero)
            //{
            //    await Task.Delay(StartAt, _cts.Token);
            //}
            //Console.WriteLine(Environment.TickCount - s);
            int repeats = 0;
            #region while
            while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    await TaskFunc();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                repeats++;
                if (repeats == Repeats) break;
                //await Task.Delay(Interval, _cts.Token);
            }
            #endregion

            #region Timer
            //_timer.Change(TimeSpan.Zero, Interval);
            #endregion
            Stop();
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