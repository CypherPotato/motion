using Motion.Parser;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Motion.Runtime;

/// <summary>
/// Represents an runtime collection of items that is used by the Motion language.
/// </summary>
/// <typeparam name="TValue">The type of items.</typeparam>
public class MotionCollection<TValue> : IDictionary<string, TValue>
{
    internal Dictionary<string, TValue> _m = new(AtomBase.SymbolComparer);
    private string? _namespace;

    /// <summary>
    /// Gets an boolean indicating if values can be inserted in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    public bool CanInsert { get; private set; }

    /// <summary>
    /// Gets an boolean indicating if values can be modified or removed in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    public bool CanEdit { get; private set; }

    /// <summary>
    /// Gets the owner <see cref="ExecutionContext"/> of this collection.
    /// </summary>
    public ExecutionContext Context { get; private set; }

    /// <inheritdoc/>
    public ICollection<TValue> Values => ((IDictionary<string, TValue>)_m).Values;

    /// <inheritdoc/>
    public int Count => ((ICollection<KeyValuePair<string, TValue>>)_m).Count;

    /// <inheritdoc/>
    public bool IsReadOnly => (CanEdit && CanInsert) == false;

    /// <inheritdoc/>
    public ICollection<string> Keys => ((IDictionary<string, TValue>)_m).Keys;

    /// <inheritdoc/>
    public TValue this[string key]
    {
        get
        {
            return Get(key);
        }
        set
        {
            Set(key, value);
        }
    }

    internal MotionCollection(ExecutionContext context, bool canInsert, bool canEdit)
    {
        Context = context;
        CanInsert = canInsert;
        CanEdit = canEdit;
    }

    internal void StartNamespace(string? @namespace)
    {
        if (string.IsNullOrEmpty(@namespace)) return;
        _namespace = @namespace;
    }

    internal void EndNamespace()
    {
        _namespace = null;
    }

    string FormatInsertingKey(string key)
    {
        if (_namespace == null)
        {
            return key;
        }
        else
        {
            return $"{_namespace}:{key}";
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The name of the value to return.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if this <see cref="MotionCollection{TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        => _m.TryGetValue(key, out value);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The defined symbol name in this collection.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TValue Get(string key)
    {
        if (TryGetValue(key, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"The name {key} is not defined in this context.");
    }

    /// <summary>
    /// Gets the value associated with the specified key or the default value if not found/defined.
    /// </summary>
    /// <param name="key">The defined symbol name in this collection.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TValue? GetOrDefault(string key)
    {
        if (TryGetValue(key, out var value))
        {
            return value;
        }

        return default;
    }

    /// <summary>
    /// Adds or sets the specified name in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="newValue">The object value.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Set(string key, TValue newValue)
    {
        string _key = FormatInsertingKey(key);

        if (ContainsKey(_key) && CanEdit == false)
        {
            throw exCannotEdit();
        }

        _m[_key] = newValue;
    }

    /// <summary>
    /// Adds the specified name and value in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="value">The object value.</param>
    public void Add(string key, TValue value)
    {
        string _key = FormatInsertingKey(key);

        if (!CanInsert)
        {
            throw exCannotAdd();
        }
        if (Context.IsSymbolDefined(_key))
        {
            throw exAlreadyDefined();
        }

        _m.Add(_key, value);
    }

    internal void InternalSet(string key, TValue value)
    {
        _m[FormatInsertingKey(key)] = value;
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, TValue>)_m).ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        if (!CanEdit)
        {
            return false;
        }
        return ((IDictionary<string, TValue>)_m).Remove(key);
    }

    /// <summary>
    /// Removes all items that the predicate match in each item's symbol.
    /// </summary>
    /// <param name="predicate">The predicate which will be executed on each symbol.</param>
    /// <returns>A integer indicating how many items were removed.</returns>
    public int RemoveAll(Func<string, bool> predicate)
    {
        if (!CanEdit)
        {
            return 0;
        }

        int count = 0;
        foreach (string key in Keys.ToArray())
            if (predicate(key))
                count += _m.Remove(key) ? 1 : 0;

        return count;
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (!CanEdit)
        {
            throw exCannotEdit();
        }
        ((ICollection<KeyValuePair<string, TValue>>)_m).Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, TValue> item)
    {
        return ((ICollection<KeyValuePair<string, TValue>>)_m).Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, TValue>>)_m).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, TValue> item)
    {
        if (!CanEdit)
        {
            throw exCannotEdit();
        }
        return ((ICollection<KeyValuePair<string, TValue>>)_m).Remove(item);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, TValue>>)_m).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_m).GetEnumerator();
    }

    InvalidOperationException exCannotEdit()
    {
        return new InvalidOperationException("It's not possible to edit or remove existing elements in this collection.");
    }

    InvalidOperationException exAlreadyDefined()
    {
        return new InvalidOperationException("This symbol is already defined in this context.");
    }

    InvalidOperationException exCannotAdd()
    {
        return new InvalidOperationException("It's not possible to add elements to this collection.");
    }
}
