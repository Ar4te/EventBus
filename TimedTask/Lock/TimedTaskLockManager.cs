using System.Collections.Concurrent;

namespace TimedTask.Lock;

public sealed class TimedTaskLockManager
{
    private TimedTaskLockManager() { }

    private static readonly ConcurrentDictionary<Guid, TimedTaskLock> _timedTaskLocks = new();

    private static TimedTaskLock RegisterLock(Guid timedTaskId)
    {
        TimedTaskLock timedTaskLock = _timedTaskLocks.GetOrAdd
             (
                 timedTaskId,
                 _timedTaskId => new TimedTaskLock(_timedTaskId)
             );

        return timedTaskLock;
    }

    public static void UnregisterLock(Guid timedTaskId)
    {
        if (_timedTaskLocks.TryRemove(timedTaskId, out var timedTaskLock))
            timedTaskLock?.Dispose();
    }

    public static async Task<TimedTaskLock> GetLockAsync(Guid timedTaskId)
    {
        if (_timedTaskLocks.TryGetValue(timedTaskId, out TimedTaskLock? timedTaskLock) && timedTaskLock is not null)
        {
            await timedTaskLock.WaitAsync();
        }
        else
        {
            timedTaskLock = RegisterLock(timedTaskId);
            await timedTaskLock.WaitAsync();
        }

        return timedTaskLock;
    }
}
