using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusCSExpert.Common.Storage;

/// <summary> Реализация базового хранилища </summary>
public class SimpleStore : IStoragable
{
    private readonly Dictionary<string, byte[]> _keyValuePairs;

    public SimpleStore()
    {
        _keyValuePairs = new Dictionary<string, byte[]>();
    }

    public void Delete(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        _keyValuePairs.Remove(key);
    }

    public byte[] Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        if (!_keyValuePairs.TryGetValue(key, out byte[]? value))
            throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary.");

        return value;
    }

    public void Set(string key, byte[] value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        _keyValuePairs[key] = value;
    }
}
