using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an runtime collection of items that is used by the Motion language.
/// </summary>
/// <typeparam name="TValue">The type of items.</typeparam>
public class MotionCollection<TValue>
{
    private Dictionary<string, TValue> _m = new Dictionary<string, TValue>(StringComparer.InvariantCultureIgnoreCase);
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
    /// Gets an array of keys on this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    public string[] Keys { get => _m.Keys.ToArray(); }

    /// <summary>
    /// Gets the owner <see cref="ExecutionContext"/> of this collection.
    /// </summary>
    public ExecutionContext Context { get; private set; }

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
    /// Gets an boolean indicating if the specified name is defined in this collection at this current scope.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    public bool Contains(string name)
        => _m.ContainsKey(name);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The name of the value to return.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if this <see cref="MotionCollection{TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, out TValue? value)
    {
        return _m.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The defined symbol name in this collection.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TValue Get(string key)
    {
        if (!Contains(key))
        {
            throw new InvalidOperationException($"The name {key} is not defined in this context.");
        }
        return _m[key];
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
        if (_m.ContainsKey(_key))
        {
            if (CanEdit)
            {
                _m[_key] = newValue;
            }
            else
            {
                throw new InvalidOperationException("Cannot change the value of this object in this context.");
            }
        }
        else
        {
            Add(key, newValue);
        }
    }

    /// <summary>
    /// Adds the specified name and value in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="value">The object value.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Add(string key, TValue value)
    {
        string _key = FormatInsertingKey(key);

        if (!CanInsert)
        {
            throw new InvalidOperationException("Cannot define other values of this kind in this context.");
        }
        if (Context.IsSymbolDefined(_key))
        {
            throw new InvalidOperationException("This symbol is already defined in this context.");
        }

        _m.Add(_key, value);
    }
}
