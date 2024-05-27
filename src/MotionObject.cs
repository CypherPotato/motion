using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion;

/// <summary>
/// Represents an runtime object which is created throught "make-object" on Motion code.
/// </summary>
public class MotionObject : IDictionary<string, object?>
{
    private IDictionary<string, object?> data = new Dictionary<string, object?>();

    /// <inheritdoc/>
    public object? this[string key] { get => data[key]; set => data[key] = value; }

    /// <inheritdoc/>
    public ICollection<string> Keys => data.Keys;

    /// <inheritdoc/>
    public ICollection<object?> Values => data.Values;

    /// <inheritdoc/>
    public int Count => data.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => data.IsReadOnly;

    /// <inheritdoc/>
    public void Add(string key, object? value)
    {
        data.Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, object?> item)
    {
        data.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        data.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, object?> item)
    {
        return data.Contains(item);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return data.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        data.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        return data.Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, object?> item)
    {
        return data.Remove(item);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        return data.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)data).GetEnumerator();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return '(' + string.Join('\n', data.Select(f => $"\t:{f.Key} <<{f.Value}>>")) + ')';
    }
}
