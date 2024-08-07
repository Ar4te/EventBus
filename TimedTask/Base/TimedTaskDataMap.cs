﻿using System.Collections.Concurrent;

namespace TimedTask.Base;

public sealed class TimedTaskDataMap : ConcurrentDictionary<string, object>
{
    public void Put<T>(string key, T value)
    {
        if (!TryAdd(key, value))
        {
            this[key] = value;
        }
    }

    public T? Get<T>(string key)
    {
        return TryGetValue(key, out var value) ? (T)value : default;
    }
}