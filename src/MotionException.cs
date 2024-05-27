﻿using System;
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
            Print(ConsoleColor.Blue, $"{' ',4} | ");
            Console.WriteLine();

            string[] lines = sourceCode.Split('\n', error.Line + 1);
            string lineText = lines[error.Line - 1];

            int icol = Math.Max(0, Math.Min(error.Column - 1, lineText.Length - 1));

            string before = lineText.Substring(0, icol);
            string current = lineText.Substring(icol, error.Length);
            string after = lineText.Substring(icol + error.Length);

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
}

internal class MotionExitException : Exception
{
    public object? Result { get; set; }
    public MotionExitException(object? data) : base() { Result = data; }
}