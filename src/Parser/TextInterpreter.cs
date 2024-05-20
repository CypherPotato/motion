using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

struct TextInterpreterSnapshot
{
    public readonly int Line;
    public readonly int Column;
    public readonly int Position;
    public readonly string LineText;
    public readonly string? Filename;
    public int Length;

    public TextInterpreterSnapshot(int line, int column, int position, int length, string lineText, string? filename)
    {
        Line = line;
        Column = column;
        Length = length;
        Position = position;
        LineText = lineText;
        Filename = filename;
    }
}

class TextInterpreter
{
    public string InputString { get; private set; }
    public int Position { get; private set; } = 0;
    public int Length { get; private set; }
    public int Line { get; private set; } = 1;
    public string? Filename { get; set; }

    public int Column
    {
        get
        {
            int col = 1;
            for (int n = 0; n < Position; n++)
            {
                col++;
                char m = InputString[n];
                if (m == '\n')
                {
                    col = 1;
                }
            }
            return col;
        }
    }

    public string CurrentLine
    {
        get
        {
            return InputString.Split('\n')[Line - 1];
        }
    }

    public TextInterpreter(string source, string? filename)
    {
        InputString = source;
        Length = InputString.Length;
        Filename = filename;
    }

    public TextInterpreterSnapshot TakeSnapshot(int length)
    {
        return new TextInterpreterSnapshot(Line, Column, Position, length, CurrentLine, Filename);
    }

    public bool CanRead()
    {
        return Position < InputString.Length;
    }

    public void Move(int count)
    {
        int moved = 0, incr = count > 0 ? 1 : -1;
        while (moved < Math.Abs(count) && Position > 0)
        {
            moved++;
            Position += incr;
            if (InputString[Position] == '\n')
            {
                Line += incr;
            }
        }
    }

    public int Read(out char c)
    {
        if (InputString.Length <= Position)
        {
            c = '\0';
            return -1;
        }
        c = InputString[Position];
        Position++;
        if (c == '\n') Line++;
        return 1;
    }

    public string ReadAtLeast(int count)
    {
        StringBuilder sb = new StringBuilder();

        int n = 0;
        while (n < count)
        {
            int j = Read(out char c);
            if (j >= 0)
            {
                sb.Append(c);
                n++;
            }
            else break;
        }

        return sb.ToString();
    }

    public char ReadUntil(Span<char> untilChars, bool wrapStringToken, out string result)
    {
        char hit = '\0';
        StringBuilder sb = new StringBuilder();

        bool inString = false;
        char b = '\0';

        while (Read(out char c) > 0)
        {
            if (wrapStringToken && c == AtomBase.Ch_StringQuote && b != '\\')
            {
                inString = !inString;
            }

            if (inString)
            {
                sb.Append(c);
                b = c;
                continue;
            }

            if (untilChars.Contains(c))
            {
                hit = c;
                break;
            }

            sb.Append(c);
            b = c;
        }

        result = sb.ToString();
        return hit;
    }

    public void SkipIgnoreTokens()
    {
        bool skipping = true;
        while (skipping)
        {
            if (Read(out char c) > 0)
            {
                if (IsIgnoreToken(c))
                {
                    continue; // whitespace
                }
                else
                {
                    Move(-1);
                    break;
                }
            }
            else break;
        }
    }

    public static bool IsIgnoreToken(char c)
    {
        return c == ' ' || c == '\t' || c == '\r' || c == '\t' || c == '\n';
    }
}
