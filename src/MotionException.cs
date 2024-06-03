using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Parser.V2;
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
    /// Gets the file which raised the exception.
    /// </summary>
    public string? Filename { get => snapshot.Filename; }

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
    /// Throws an <see cref="MotionException"/> indicating that the provided Atom value is null.
    /// </summary>
    /// <param name="atom">The atom where the value is null.</param>
    public static MotionException CreateNullParameter(Atom atom)
    {
        return new MotionException("This atom cannot result in a null value.", atom);
    }

    /// <summary>
    /// Throws an <see cref="MotionException"/> indicating that the provided Atom value is not compatible
    /// with the desired type.
    /// </summary>
    /// <param name="atom">The atom where the value is null.</param>
    /// <param name="expectedType">The expected type of the atom result.</param>
    public static MotionException CreateIncorrectType(Atom atom, Type expectedType)
    {
        return new MotionException($"This atom is expected to have an '{expectedType.FullName}' object.", atom);
    }

    /// <summary>
    /// Writes an well-formatted error explanation of the specified <see cref="MotionException"/>
    /// into an string.
    /// </summary>
    /// <param name="sourceCode">The source code where the exception was originated.</param>
    /// <param name="error">The exception object.</param>
    public static string MakeErrorMessage(string? sourceCode, MotionException error)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("at ");
        sb.Append(error.Filename ?? "<snippet>");
        sb.Append(", line ");
        sb.Append(error.Line);
        sb.Append(", col. ");
        sb.Append(error.Column);
        sb.AppendLine(":");

        if (sourceCode is null)
        {
            sb.Append(error.Message);
            sb.AppendLine();
        }
        else
        {
            sb.Append($"{' ',4} | ");
            sb.AppendLine();

            string[] lines = sourceCode.Split('\n', error.Line + 1);
            string lineText = lines[error.Line - 1];

            int icol = Math.Max(0, Math.Min(error.Column - 1, lineText.Length - 1));

            string before = lineText.Substring(0, icol);
            string current = lineText.Substring(icol, error.Length);
            string after = lineText.Substring(icol + error.Length);

            sb.Append($"{error.Line,4} | ");
            sb.Append(before);
            sb.Append(current);
            sb.Append(after);
            sb.AppendLine();
            sb.Append($"{' ',4} | ");
            sb.Append(new string(' ', error.Column - 1));
            sb.Append(new string('-', error.Length));
            sb.AppendLine();

            foreach (string line in error.Message.Split('\n'))
            {
                sb.Append($"{' ',4} : ");
                sb.Append(new string(' ', error.Column - 1));
                sb.Append(line);
                sb.AppendLine();
            }
            sb.Append($"{' ',4} : \n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Write a colorful, well-formatted message for an error message.
    /// </summary>
    /// <param name="sourceCode">The source code where this message originated.</param>
    /// <param name="error">The error object.</param>
    public static void DumpErrorMessage(string? sourceCode, MotionException error)
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

        if (sourceCode is null)
        {
            Print(ConsoleColor.Red, error.Message);
            Console.WriteLine();
        }
        else
        {
            try
            {
                string[] lines = sourceCode.Split('\n', error.Line + 1);
                string lineText = lines[error.Line - 1];

                int icol = Math.Max(0, Math.Min(error.Column - 1, lineText.Length - 1));

                string before = lineText.Substring(0, icol);
                string current = lineText.Substring(icol, error.Length);
                string after = lineText.Substring(icol + error.Length);

                Print(ConsoleColor.Blue, $"{' ',4} | ");
                Console.WriteLine();
                Print(ConsoleColor.Blue, $"{error.Line,4} | ");
                Console.Write(before);
                Print(ConsoleColor.Red, current);
                Console.Write(after);
                Console.WriteLine();
                Print(ConsoleColor.Blue, $"{' ',4} | ");
                Print(ConsoleColor.Red, new string(' ', error.Column - 1));
                Print(ConsoleColor.DarkRed, new string('-', error.Length));
                Console.WriteLine();
            }
            catch
            {
                Print(ConsoleColor.Red, error.Message);
                Console.WriteLine();
                return;
            }           

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
}

internal class MotionExitException : Exception
{
    public object? Result { get; set; }
    public MotionExitException(object? data) : base() { Result = data; }
}