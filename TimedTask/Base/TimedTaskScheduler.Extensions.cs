using System.Collections.Concurrent;

namespace TimedTask.Base;

public sealed class TimedTaskLockManager
{
    private static readonly ConcurrentDictionary<Guid, TimedTaskLock> _timedTaskLocks = new();

    public static TimedTaskLock RegisterLock(Guid timedTaskId)
    {
        TimedTaskLock timedTaskLock = _timedTaskLocks.AddOrUpdate(
                 timedTaskId,
                 _ => new TimedTaskLock(timedTaskId),
                 (_, oldValue) => oldValue
             );

        return timedTaskLock;
    }

    public static void UnregisterLock(Guid timedTaskId)
    {
        _timedTaskLocks.TryRemove(timedTaskId, out var timedTaskLock);
        timedTaskLock?.Dispose();
    }

    public static async Task<TimedTaskLock> GetLockAsync(Guid timedTaskId)
    {
        if (_timedTaskLocks.TryGetValue(timedTaskId, out TimedTaskLock? timedTaskLock) &&
            timedTaskLock is not null)
        {
            await Task.Run(() =>
             {
                 while (!timedTaskLock.IsLocked)
                 {
                     break;
                 }
             });
        }
        else
        {
            timedTaskLock = RegisterLock(timedTaskId);
        }

        timedTaskLock.Lock();
        return timedTaskLock;
    }
}

public class TimedTaskLock : IDisposable
{
    public Guid TimedTaskId { get; set; }

    public bool IsLocked { get; set; }

    public TimedTaskLock(Guid timedTaskId)
    {
        TimedTaskId = timedTaskId;
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Release()
    {
        IsLocked = false;
    }

    public void Dispose()
    {
        this?.Release();
        GC.SuppressFinalize(this);
    }
}