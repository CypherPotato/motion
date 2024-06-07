using Motion.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an atom in Motion code, which can be a symbol, number, string, group of atoms or other data type.
/// </summary>
public struct Atom
{
    internal AtomBase _ref;
    internal AtomBase _parent;
    private bool wasEvaluated;
    private object? evaluatedResult;

    /// <summary>
    /// Gets the execution context to which this atom belongs.
    /// </summary>
    public ExecutionContext Context { get; }

    internal Atom(AtomBase refToken, AtomBase refParent, ExecutionContext context)
    {
        _ref = refToken;
        _parent = refParent;
        Context = context;
    }

    object? ResolveTokenValue()
    {
        if (!wasEvaluated)
        {
            evaluatedResult = Context.EvaluateTokenItem(_ref, _parent);
            wasEvaluated = true;
        }
        return evaluatedResult;
    }


    AtomBase GetRef() => _ref.Type == TokenType.Undefined ? _parent : _ref;


    T ThrowInvalidTkType<T>(string requestedType)
    {
        throw new MotionException($"Cannot cast the current atom value. Expected {requestedType}, but got {_ref.Type}.", GetRef().Location, null);
    }

    T ThrowNullObject<T>(string requestedType)
    {
        throw new MotionException($"Error caught when trying to read expected {requestedType}, which got NIL instead.", GetRef().Location, null);
    }

    /// <summary>
    /// Gets an boolean indicating if the specified keyword is defined or not in this <see cref="Atom"/>.
    /// </summary>
    /// <param name="keyword">The keyword value.</param>
    public bool HasKeyword(string keyword)
    {
        return _ref.SingleKeywords.Contains(keyword, AtomBase.SymbolComparer);
    }

    internal static AtomType AtomTypeFromTokenType(TokenType type)
    {
        return type switch
        {
            TokenType.Undefined or TokenType.Null => AtomType.Null,
            TokenType.String => AtomType.StringLiteral,
            TokenType.Character => AtomType.CharacterLiteral,
            TokenType.Number => AtomType.NumberLiteral,
            TokenType.Boolean => AtomType.BooleanLiteral,
            TokenType.Expression => AtomType.Expression,
            TokenType.Array => AtomType.Array,
            TokenType.Symbol => AtomType.Symbol,
            TokenType.Keyword => AtomType.Keyword,
            TokenType.Operator => AtomType.Operator,
            TokenType.ClrSymbol => AtomType.ClrSymbol,
            TokenType.ClrType => AtomType.ClrType,
            _ => AtomType.Null
        };
    }

    /// <summary>
    /// Gets the <see cref="AtomType"/> of the current atom.
    /// </summary>
    public AtomType Type
    {
        get => AtomTypeFromTokenType(_ref.Type);
    }

    /// <summary>
    /// Gets all keywords defined in this atom.
    /// </summary>
    public readonly string[] Keywords { get => _ref.SingleKeywords; }

    /// <summary>
    /// Gets the number of child atoms contained in this atom.
    /// </summary>
    public readonly int ItemCount { get => Math.Max(_ref.Children.Length, 1); }

    /// <summary>
    /// Ensures that this atom has at least the specified number of child atoms. Throws an <see cref="InvalidOperationException"/> if the condition is not met.
    /// </summary>
    /// <param name="count">The minimum required number of child atoms.</param>
    public void EnsureMinimumItemCount(int count)
    {
        if (ItemCount < count)
        {
            throw new MotionException($"The atom {_ref} expects at least {count} atoms. Got {ItemCount} instead.", GetRef().Location, null);
        }
    }

    /// <summary>
    /// Ensures that this atom has exactly the specified number of child atoms. Throws an <see cref="InvalidOperationException"/> if the condition is not met.
    /// </summary>
    /// <param name="count">The exact required number of child atoms.</param>
    public void EnsureExactItemCount(int count)
    {
        if (ItemCount != count)
        {
            throw new MotionException($"The atom {_ref} expects exacts {count} atoms. Got {ItemCount} instead.", GetRef().Location, null);
        }
    }

    /// <summary>
    /// Ensures that this atom has exactly the specified number of child atoms. Throws an <see cref="InvalidOperationException"/> if the condition is not met.
    /// </summary>
    /// <param name="allowedCounts">The exact required number of child atoms.</param>
    public void EnsureExactItemCount(params int[] allowedCounts)
    {
        if (!allowedCounts.Contains(ItemCount))
        {
            string n;
            if (allowedCounts.Length == 1)
            {
                n = $"{allowedCounts[0]}";
            }
            else if (allowedCounts.Length == 2)
            {
                n = $"{allowedCounts[0]} or {allowedCounts[1]}";
            }
            else if (allowedCounts.Length >= 3)
            {
                n = $"{string.Join(", ", allowedCounts[1..(allowedCounts.Length - 1)])} or {allowedCounts[..1]}";
            }
            else
            {
                n = "no";
            }
            throw new MotionException($"The atom {_ref} expects {n} atoms. Got {ItemCount} instead.", GetRef().Location, null);
        }
    }

    /// <summary>
    /// Ensures that this <see cref="Atom"/> doens't has any child atoms.
    /// </summary>
    public void EnsureParameterless()
    {
        if (ItemCount > 1)
        {
            throw new MotionException($"The atom {_ref} doens't expects any other atoms.", GetRef().Location, null);
        }
    }

    /// <summary>
    /// Gets an enumerable collection of child atoms contained in this atom.
    /// </summary>
    /// <returns>An enumerable collection of child atoms.</returns>
    public IEnumerable<Atom> GetAtoms()
    {
        for (int i = 0; i < _ref.Children.Length; i++)
        {
            yield return new Atom(_ref.Children[i], _ref, Context);
        }
    }

    /// <summary>
    /// Gets a child atom with the specified keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The child atom with the specified keyword, or an undefined atom if not found.</returns>
    public Atom GetAtom(string keyword)
    {
        foreach (AtomBase t in _ref.Children)
        {
            if (string.Compare(t.Keyword, keyword, true) == 0)
            {
                return new Atom(t, _ref, Context);
            }
        }
        return new Atom(AtomBase.Undefined, AtomBase.Undefined, Context);
    }

    /// <summary>
    /// Gets a child atom at the specified index.
    /// </summary>
    /// <param name="position">The zero-based index of the child atom to retrieve.</param>
    /// <returns>The child atom at the specified index, or an undefined atom if out of bounds.</returns>
    public Atom GetAtom(int position)
    {
        if (_ref.Children.Length == 0)
        {
            return ThrowInvalidTkType<Atom>("Expression");
        }
        else if (_ref.Children.Length <= position)
        {
            return new Atom(AtomBase.Undefined, AtomBase.Undefined, Context);
        }
        else
        {
            return new Atom(_ref.Children[position], _ref, Context);
        }
    }

    void ResetTokenValue()
    {
        wasEvaluated = false;
        evaluatedResult = null;
    }

    /// <summary>
    /// Checks if the atom value is null or undefined and returns either null or the atom itself.
    /// </summary>
    /// <returns>Null if the atom value is null or undefined, otherwise the atom itself.</returns>
    public Atom? Nullable()
    {
        ResetTokenValue();
        if (_ref.Type == TokenType.Null || _ref.Type == TokenType.Undefined || ResolveTokenValue() is null)
        {
            return null;
        }
        return this;
    }

    /// <summary>
    /// Gets the string value of this atom if it represents a symbol, throws an exception otherwise.
    /// </summary>
    /// <returns>The string value of the symbol.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom does not represent a symbol.</exception>
    public string GetSymbol()
    {
        if (_ref.Type == TokenType.Symbol)
        {
            return _ref.Content!.ToString()!;
        }
        else
        {
            return ThrowInvalidTkType<string>("symbol");
        }
    }

    /// <summary>
    /// Gets the string representation of the evaluated value of this atom, throws an exception if receives an null string.
    /// </summary>
    /// <returns>The string representation of the atom's value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null.</exception>
    public string GetString()
    {
        string? b = ResolveTokenValue()?.ToString();
        if (b == null)
        {
            return ThrowNullObject<string>("string");
        }
        ResetTokenValue();
        return b;
    }

    /// <summary>
    /// Gets the char representation of the evaluated value of this atom, throws an exception if receives an null string.
    /// </summary>
    /// <returns>The char representation of the atom's value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null.</exception>
    public char GetChar()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<char>("character");
        }
        ResetTokenValue();
        return Convert.ToChar(b);
    }

    /// <summary>
    /// Gets the unsigned 16-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The ushort value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public ushort GetUInt16()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<ushort>("ushort");
        }
        ResetTokenValue();
        return Convert.ToUInt16(b);
    }

    /// <summary>
    /// Gets the unsigned 32-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The uint value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public uint GetUInt32()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<uint>("uinteger");
        }
        ResetTokenValue();
        return Convert.ToUInt32(b);
    }

    /// <summary>
    /// Gets the unsigned 64-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The ulong value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public ulong GetUInt64()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<ulong>("ulong");
        }
        ResetTokenValue();
        return Convert.ToUInt64(b);
    }

    /// <summary>
    /// Gets the signed 16-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The short value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public short GetInt16()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<short>("short");
        }
        ResetTokenValue();
        return Convert.ToInt16(b);
    }

    /// <summary>
    /// Gets the signed 32-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The int value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public int GetInt32()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<int>("integer");
        }
        ResetTokenValue();
        return Convert.ToInt32(b);
    }

    /// <summary>
    /// Gets the signed 64-bit integer value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The long value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public long GetInt64()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<long>("long");
        }
        ResetTokenValue();
        return Convert.ToInt64(b);
    }

    /// <summary>
    /// Gets the float value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The float value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public float GetFloat()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<float>("float");
        }
        ResetTokenValue();
        return Convert.ToSingle(b);
    }

    /// <summary>
    /// Gets the double value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The double value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public double GetDouble()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<double>("double");
        }
        ResetTokenValue();
        return Convert.ToDouble(b);
    }

    /// <summary>
    /// Gets the byte value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The byte value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public byte GetByte()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<byte>("byte");
        }
        ResetTokenValue();
        return Convert.ToByte(b);
    }

    /// <summary>
    /// Gets the sbyte value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The sbyte value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public sbyte GetSByte()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<sbyte>("sbyte");
        }
        ResetTokenValue();
        return Convert.ToSByte(b);
    }

    /// <summary>
    /// Gets the boolean value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The boolean value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public bool GetBoolean()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<bool>("bool");
        }
        ResetTokenValue();
        return Convert.ToBoolean(b);
    }

    /// <summary>
    /// Gets the value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public object GetObject()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            return ThrowNullObject<object>("object");
        }
        ResetTokenValue();
        return b;
    }

    /// <summary>
    /// Gets the value of this atom and expects it to be an non-null <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the atom.</typeparam>
    /// <returns>An non-null <typeparamref name="T"/> object.</returns>
    public T GetObject<T>() where T : notnull
    {
        object obj = GetObject();
        if (obj is T t)
        {
            ResetTokenValue();
            return t;
        }
        else
        {
            return ThrowInvalidTkType<T>(typeof(T).Name);
        }
    }

    /// <summary>
    /// Gets the value of this atom, throws an exception if null.
    /// </summary>
    /// <returns>The value of the atom.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the atom's value is null or cannot be converted.</exception>
    public IEnumerable<object?> GetArray()
    {
        object? b = ResolveTokenValue();
        if (b == null)
        {
            yield return ThrowNullObject<IEnumerable<object?>>("array");
            yield break;
        }
        ResetTokenValue();

        IEnumerable s = (IEnumerable)b;
        foreach (object? o in s)
        {
            yield return o;
        }
    }
}
