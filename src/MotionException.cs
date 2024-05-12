using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Runtime;

namespace Motion;

/// <summary>
/// Represents an exception thrown by the Motion interpreter.
/// </summary>
public class MotionException : Exception
{
    internal TextInterpreterSnapshot snapshot;

    /// <summary>
    /// Gets the line number of the current position in the snapshot.
    /// </summary>
    public int Line { get => snapshot.Line; }

    /// <summary>
    /// Gets the character position of the current position in the snapshot.
    /// </summary>
    public int Position { get => snapshot.Position; }

    /// <summary>
    /// Gets the column number of the current position in the snapshot.
    /// </summary>
    public int Column { get => snapshot.Column; }

    /// <summary>
    /// Gets the length of the text at the current position in the snapshot.
    /// </summary>
    public int Length { get => snapshot.Length; }

    /// <summary>
    /// Gets the text of the line containing the current position in the snapshot.
    /// </summary>
    public string LineText { get => snapshot.LineText; }


    internal MotionException(string message, TextInterpreter i) : base(message)
    {
        snapshot = i.TakeSnapshot(1);
    }

    internal MotionException(string message, TextInterpreterSnapshot i, Exception? innerException) : base(message, innerException)
    {
        snapshot = i;
    }

    /// <summary>
    /// Creates an new <see cref="MotionException"/> instance from given atom and message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="atom">The <see cref="Atom"/> which raised the exception.</param>
    public MotionException(string message, Atom atom) : base(message)
    {
        snapshot = atom._ref.Location;
    }
}

internal class MotionExitException : Exception
{
    public object? Result { get; set; }
    public MotionExitException(object? data) : base() { Result = data; }
}