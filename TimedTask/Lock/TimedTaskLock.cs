namespace TimedTask.Lock;

public class TimedTaskLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private bool _disposed = false;

    public Guid TimedTaskId { get; set; }

    public bool IsLocked => _semaphore.CurrentCount == 0;

    public TimedTaskLock(Guid timedTaskId)
    {
        TimedTaskId = timedTaskId;
    }

    public async Task WaitAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TimedTaskLock));
        }
        await _semaphore.WaitAsync();
    }

    public void Release()
    {
        _semaphore.Release();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }

            _disposed = true;
        }
    }

    ~TimedTaskLock()
    {
        Dispose(false);
    }
}