using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Runtime;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public int Length { get => Math.Min(snapshot.Length, LineText.Length); }

    /// <summary>
    /// Gets the text of the line containing the current position in the snapshot.
    /// </summary>
    public string LineText { get => snapshot.LineText; }

    /// <summary>
    /// Gets the file which raised the exception.
    /// </summary>
    public string? Filename { get => snapshot.Filename; }

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

    /// <summary>
    /// Builds an pretty, well formated exception message for the specified <see cref="MotionException"/>.
    /// </summary>
    /// <param name="error">The <see cref="MotionException"/> error.</param>
    /// <returns></returns>
    public static string BuildErrorMessage(MotionException error)
    {
        StringBuilder sb = new StringBuilder();
        if (error.Filename is null)
        {
            sb.AppendLine($"at line {error.Line}, col {error.Column}:");
        }
        else
        {
            sb.AppendLine($"at file {error.Filename}, line {error.Line}, col {error.Column}:");
        }

        sb.AppendLine();
        sb.AppendLine(error.LineText);
        sb.AppendLine(new string(' ', Math.Max(0, error.Column - 1)) + new string('^', error.Length));

        foreach (string line in error.Message.Split('\n'))
        {
            sb.AppendLine(new string(' ', Math.Max(0, error.Column - 1)) + line.TrimStart());
        }

        return sb.ToString();
    }
}

internal class MotionExitException : Exception
{
    public object? Result { get; set; }
    public MotionExitException(object? data) : base() { Result = data; }
}