using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusCSExpert.Common.Storage;

/// <summary> Реализация базового хранилища </summary>
public class SimpleStore : IStoragable
{
    private readonly Dictionary<string, byte[]> _keyValuePairs = [];
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    private long _setCount;
    private long _getCount;
    private long _deleteCount;

    public void Delete(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        _lock.EnterWriteLock();
        try
        {
            _keyValuePairs.Remove(key);
            Interlocked.Increment(ref _deleteCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public byte[] Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        _lock.EnterReadLock();
        try
        {
            return !_keyValuePairs.TryGetValue(key, out byte[]? value) ? null : value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Set(string key, byte[] value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        _lock.EnterWriteLock();
        try
        {
            _keyValuePairs[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long setCount, long getCount, long deleteCount) GetStatistics() =>
        (_setCount, _getCount, _deleteCount);
}
