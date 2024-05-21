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
            sb.AppendLine($"error at line {error.Line}, col {error.Column}:");
        }
        else
        {
            sb.AppendLine($"error at file {error.Filename}, line {error.Line}, col {error.Column}:");
        }

        sb.AppendLine("     | ");
        sb.Append($"{error.Line,4} | ");
        sb.AppendLine(error.LineText);
        sb.Append("     | ");
        sb.AppendLine(new string(' ', Math.Max(0, error.Column - 1)) + new string('-', error.Length));

        foreach (string line in error.Message.Split('\n'))
        {
            sb.Append("     : ");
            sb.AppendLine(new string(' ', Math.Max(0, error.Column - 1)) + line.TrimStart());
        }

        return sb.ToString();
    }

    public static void DumpErrorMessage(MotionException error)
    {
        void Print(ConsoleColor color, object? message)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        Console.Write("at ");
        Print(ConsoleColor.Blue, error.Filename ?? "<snippet>");
        Console.Write(", line ");
        Print(ConsoleColor.Blue, error.Line);
        Console.Write(", col. ");
        Print(ConsoleColor.Blue, error.Column);
        Console.WriteLine(":");

        Print(ConsoleColor.Blue, $"{' ',4} | ");
        Console.WriteLine();

        string before = error.LineText.Substring(0, error.Column - 1);
        string current = error.LineText.Substring(error.Column - 1, error.Length);
        string after = error.LineText.Substring(error.Column - 1 + error.Length);

        Print(ConsoleColor.Blue, $"{error.Line,4} | ");
        Console.Write(before);
        Print(ConsoleColor.Red, current);
        Console.Write(after);
        Console.WriteLine();
        Print(ConsoleColor.Blue, $"{' ',4} | ");
        Print(ConsoleColor.Red, new string(' ', error.Column - 1));
        Print(ConsoleColor.DarkRed, new string('-', error.Length));
        Console.WriteLine();

        foreach (string line in error.Message.Split('\n'))
        {
            Print(ConsoleColor.DarkBlue, $"{' ',4} : ");
            Console.Write(new string(' ', error.Column - 1));
            Print(ConsoleColor.Red, line);
            Console.WriteLine();
        }
        Print(ConsoleColor.DarkBlue, $"{' ',4} : \n");
    }
}

internal class MotionExitException : Exception
{
    public object? Result { get; set; }
    public MotionExitException(object? data) : base() { Result = data; }
}