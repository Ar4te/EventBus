using System.Collections;
using System.Collections.Concurrent;

namespace Infrastructure;

public class ThreadSafeBag<T> : IEquatable<ThreadSafeBag<T>>
    where T : IEquatable<T>
{
    private readonly ConcurrentBag<T> _bag;
    private readonly object _bagLock = new();

    public ThreadSafeBag()
    {
        _bag = new ConcurrentBag<T>();
    }

    public void Add(T item)
    {
        _bag.Add(item);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ThreadSafeBag{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ThreadSafeBag{T}"/>. The value can be null for reference types.
    /// </param>
    /// <returns><see langword="true" /> if item is successfully removed; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if item was not found in the <see cref="ThreadSafeBag{T}"/>.</returns>
    public bool Remove(T item)
    {
        lock (_bagLock)
        {
            var items = new List<T>(_bag);
            if (items.Remove(item))
            {
                _bag.Clear();
                foreach (var i in items)
                {
                    _bag.Add(i);
                }
                return true;
            }
            return false;
        }
    }

    public bool Contains(T item)
    {
        return _bag.Contains(item);
    }

    public void Clear()
    {
        lock (_bagLock)
        {
            _bag.Clear();
        }
    }

    public int Count => _bag.Count;

    public IEnumerable<T> Values => new List<T>(_bag);

    public bool Equals(ThreadSafeBag<T>? other)
    {
        if (other == null) return false;

        lock (_bagLock)
        {
            if (_bag.Count != other._bag.Count) return false;

            var thisItems = _bag.OrderBy(t => t).ToArray();
            var otherItems = other._bag.OrderBy(t => t).ToArray();
            return thisItems.SequenceEqual(otherItems);
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is ThreadSafeBag<T> other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode()
    {
        lock (_bagLock)
        {
            int hash = 17;
            foreach (var item in _bag.OrderBy(t => t))
            {
                hash = hash * 31 + item?.GetHashCode() ?? 0;
            }
            return hash;
        }
    }
}