namespace TimedTask.Lock;

public class TimedTaskLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private bool _disposedValue;

    public Guid TimedTaskId { get; set; }

    public bool IsLocked => _semaphore.CurrentCount == 0;

    public TimedTaskLock(Guid timedTaskId)
    {
        TimedTaskId = timedTaskId;
    }

    public async Task WaitAsync()
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(TimedTaskLock));
        }
        await _semaphore.WaitAsync();
    }

    public void Release()
    {
        _semaphore.Release();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }
            _disposedValue = true;
        }
    }

    ~TimedTaskLock()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}