using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser.V2;

internal class TextInterpreter : IDisposable
{
    private TextReader reader;
    private TextPosition position;
    private char current;
    private TextInterpreterExceptionManager expManager;
    private string? fileName;

    public TextInterpreterExceptionManager ExceptionManager { get => expManager; }

    public TextPosition Position
    {
        get => position;
    }

    public bool CanRead
    {
        get => this.reader.Peek() != -1;
    }

    public char Current
    {
        get => current;
    }

    public TextInterpreterSnapshot GetSnapshot(int length) => new TextInterpreterSnapshot(position.line + 1, position.column + 1, position.index, length, fileName);

    public TextInterpreter(string? fileName, TextReader reader)
    {
        this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        this.current = '\0';
        this.position = new TextPosition();
        this.expManager = new TextInterpreterExceptionManager(this);
        this.fileName = fileName;
    }

    public char Peek()
    {
        var next = reader.Peek();

        if (next == -1)
        {
            return '\0';
        }

        return (char)next;
    }

    public char Read()
    {
        var next = reader.Read();

        if (next == -1)
        {
            throw ExceptionManager.NotFinishedString();
        }

        this.position.index += 1;
        this.current = (char)next;

        switch (next)
        {
            case '\r':
                // Normalize '\r\n' line encoding to '\n'.
                if (reader.Peek() == '\n')
                {
                    this.position.index += 1;
                    reader.Read();
                }
                goto case '\n';

            case '\n':
                this.position.line += 1;
                this.position.column = 0;
                return '\n';

            default:
                this.position.column += 1;
                return (char)next;
        }
    }

    public void SkipComment()
    {
        var peek = Peek();

        if (peek == AtomBase.Ch_CommentChar)
        {
            bool ignoring = true;
            while (ignoring && CanRead)
            {
                if (IsNewLine(Peek()))
                {
                    break;
                }
                else
                {
                    Read();
                }
            }
        }
    }

    public void SkipWhitespace()
    {
        while (IsWhiteSpace(Peek()))
        {
            Read();
        }
    }

    public char AssertAny(params char[] next)
    {
        var p = Peek();
        for (int i = 0; i < next.Length; i++)
        {
            if (next[i] == p)
                return p;
        }

        throw ExceptionManager.UnexpectedToken(p.ToString());
    }

    public void Assert(char next)
    {
        if (Peek() == next)
        {
            Read();
        }
        else
        {
            throw ExceptionManager.ExpectToken(next.ToString());
        }
    }

    public void Dispose()
    {
        reader.Dispose();
    }

    public static bool IsWhiteSpace(char c)
        => c == ' ' || c == '\t' || c == '\n' || c == '\r';

    public static bool IsNewLine(char c)
        => c == '\n' || c == '\r';
}
