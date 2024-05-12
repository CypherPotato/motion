using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

public class MotionCollection<TValue>
{
    private Dictionary<string, TValue> _m = new Dictionary<string, TValue>(StringComparer.InvariantCultureIgnoreCase);
    private string? _namespace;

    public bool CanInsert { get; private set; }
    public bool CanEdit { get; private set; }
    public string[] Keys { get => _m.Keys.ToArray(); }

    public ExecutionContext Context { get; private set; }

    public MotionCollection(ExecutionContext context, bool canInsert, bool canEdit)
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

    public bool IsSymbolDefined(string name)
    {
        return Context.Variables.Contains(name)
            || Context.Constants.Contains(name)
            || Context.UserFunctions.Contains(name)
            || Context.Methods.Contains(name);
    }

    public bool Contains(string name)
        => _m.ContainsKey(name);

    public void Lock()
    {
        CanInsert = false;
        CanEdit = false;
    }

    public bool TryGetValue(string key, out TValue? value)
    {
        if (_m.ContainsKey(key))
        {
            value = _m[key];
            return true;
        }
        value = default;
        return false;
    }

    public TValue Get(string key)
    {
        if (!Contains(key))
        {
            throw new InvalidOperationException($"The name {key} is not defined in this context.");
        }
        return _m[key];
    }

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

    public void Add(string key, TValue value)
    {
        string _key = FormatInsertingKey(key);

        if (!CanInsert)
        {
            throw new InvalidOperationException("Cannot define other values of this kind in this context.");
        }
        if (IsSymbolDefined(_key))
        {
            throw new InvalidOperationException("This symbol is already defined in this context.");
        }

        _m.Add(_key, value);
    }
}
