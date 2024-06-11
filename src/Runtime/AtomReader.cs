using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents a reader that can read a sequential series of <see cref="Atom"/>.
/// </summary>
public class AtomReader : IEnumerator<Atom>, IEnumerable<Atom>
{
    private Atom _ref;
    private int position = 0;

    /// <summary>
    /// Creates an new <see cref="AtomReader"/> instance with the specified
    /// <see cref="Atom"/> to read.
    /// </summary>
    /// <param name="atom">The expression-type <see cref="Atom"/> to read.</param>
    public AtomReader(Atom atom)
    {
        if (atom.Type != AtomType.Expression)
        {
            throw new InvalidOperationException("To read this atom, it must be of type expression.");
        }
        _ref = atom;
    }

    /// <summary>
    /// Indicates if the current reader can read more atoms.
    /// </summary>
    public bool CanRead { get => Peek()._ref.Type != Parser.TokenType.Undefined; }

    /// <summary>
    /// Gets the current <see cref="Atom"/> object.
    /// </summary>
    public Atom Current { get => _ref.GetAtom(position); }

    /// <summary>
    /// Gets the current reader position index.
    /// </summary>
    public int Position { get => position; }

    /// <summary>
    /// Gets the count of available atoms to read in this reader.
    /// </summary>
    public int Count { get => _ref.ItemCount; }

    object IEnumerator.Current => Current;

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        position = 0;
        _ref = default;
    }

    /// <summary>
    /// Advances the current reader position by one.
    /// </summary>
    /// <returns>An boolean indicating whether the reader can read more atoms or not.</returns>
    public bool MoveNext()
    {
        if (CanRead)
        {
            Interlocked.Increment(ref position);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Peeks the next atom without changing the current position.
    /// </summary>
    /// <returns>
    /// An <see cref="Atom"/> object. Determine if this atom represents an valid atom inside the
    /// parent <see cref="Atom"/> asserting <see cref="Atom.IsUndefined"/>.
    /// </returns>
    public Atom Peek()
    {
        Atom pAtom = _ref.GetAtom(position + 1);
        return pAtom;
    }

    /// <summary>
    /// Resets this reader position to zero.
    /// </summary>
    public void Reset()
    {
        position = -1;
    }

    /// <inheritdoc/>
    public IEnumerator<Atom> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }
}
