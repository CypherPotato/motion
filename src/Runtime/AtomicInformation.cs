using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an object which is present in the Motion code.
/// </summary>
/// <typeparam name="TValue">The type of the object.</typeparam>
public class AtomicInformation<TValue>
{
    TextInterpreterSnapshot snapshot;

    /// <summary>
    /// Gets the information name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the information value.
    /// </summary>
    public TValue Value { get; private set; }

    /// <summary>
    /// Gets the line number of the current position in the snapshot.
    /// </summary>
    public int Line { get => snapshot.Line; }

    /// <summary>
    /// Gets the character position of the current position in the snapshot.
    /// </summary>
    public int Position { get => snapshot.Index; }

    /// <summary>
    /// Gets the column number of the current position in the snapshot.
    /// </summary>
    public int Column { get => snapshot.Column; }

    /// <summary>
    /// Gets the length of the text at the current position in the snapshot.
    /// </summary>
    public int Length { get => snapshot.Length; }

    /// <summary>
    /// Gets the file where this value was defined.
    /// </summary>
    public string Filename { get => snapshot.Filename ?? "<core>"; }

    internal void SetValue(TValue value) => Value = value;

    internal AtomicInformation(TextInterpreterSnapshot parent, string name, TValue value)
    {
        snapshot = parent;
        Name = name;
        Value = value;
    }
}
