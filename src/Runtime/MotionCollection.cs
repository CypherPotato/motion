using Motion.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Motion.Runtime;

/// <summary>
/// Represents an runtime collection of items that is used by the Motion language.
/// </summary>
/// <typeparam name="TValue">The type of items.</typeparam>
public class MotionCollection<TValue> : IEnumerable<AtomicInformation<TValue>>
{
    internal List<AtomicInformation<TValue>> _m = new();
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
    public string[] Keys { get => GetKeys().ToArray(); }

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

    internal IEnumerable<string> GetKeys()
    {
        foreach (var at in _m)
            yield return at.Name;
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
    {
        foreach (var at in _m)
            if (string.Compare(at.Name, name, true) == 0)
                return true;

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The name of the value to return.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if this <see cref="MotionCollection{TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, out TValue? value)
    {
        foreach (var at in _m)
            if (string.Compare(at.Name, key, true) == 0)
            {
                value = at.Value;
                return true;
            }

        value = default(TValue);
        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The defined symbol name in this collection.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TValue Get(string key)
    {
        foreach (var at in _m)
            if (string.Compare(at.Name, key, true) == 0)
                return at.Value;

        throw new InvalidOperationException($"The name {key} is not defined in this context.");
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The defined symbol name in this collection.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public AtomicInformation<TValue> GetAtomicReference(string key)
    {
        foreach (var at in _m)
            if (string.Compare(at.Name, key, true) == 0)
                return at;

        throw new InvalidOperationException($"The name {key} is not defined in this context.");
    }

    /// <summary>
    /// Adds or sets the specified name in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="newValue">The object value.</param>
    public void Set(string key, TValue value) => Set(new Atom(AtomBase.Undefined, AtomBase.Undefined, Context), key, value);

    /// <summary>
    /// Adds or sets the specified name in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="newValue">The object value.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Set(Atom declaringAtom, string key, TValue newValue)
    {
        string _key = FormatInsertingKey(key);

        foreach (var at in _m)
            if (string.Compare(at.Name, key, true) == 0)
            {
                if (!CanEdit)
                {
                    throw new InvalidOperationException("Cannot change the value of this object in this context.");
                }

                at.SetValue(newValue);
                return;
            }

        Add(declaringAtom, key, newValue);
    }

    /// <summary>
    /// Adds the specified name and value in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="value">The object value.</param>
    public void Add(string key, TValue value) => Add(new Atom(AtomBase.Undefined, AtomBase.Undefined, Context), key, value);

    /// <summary>
    /// Adds the specified name and value in this <see cref="MotionCollection{TValue}"/>.
    /// </summary>
    /// <param name="key">The symbol name.</param>
    /// <param name="value">The object value.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Add(Atom declaringAtom, string key, TValue value)
    {
        Add(declaringAtom._ref.Location, key, value);
    }

    internal void Add(TextInterpreterSnapshot location, string key, TValue value)
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

        _m.Add(new AtomicInformation<TValue>(location, _key, value));
    }

    internal void InternalSet(TextInterpreterSnapshot location, string key, TValue value)
    {
        foreach (var at in _m)
            if (string.Compare(at.Name, key, true) == 0)
            {
                at.SetValue(value);
                return;
            }

        _m.Add(new AtomicInformation<TValue>(location, key, value));
    }

    /// <inheritdoc/>
    public IEnumerator<AtomicInformation<TValue>> GetEnumerator()
    {
        return ((IEnumerable<AtomicInformation<TValue>>)_m).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_m).GetEnumerator();
    }
}
