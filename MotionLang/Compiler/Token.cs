using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Compiler;

struct Token
{
    public static readonly char Ch_ExpressionStart = '(';
    public static readonly char Ch_ExpressionEnd = ')';
    public static readonly char Ch_StringQuote = '"';

    private static readonly char[] AllowedFirstSymbolChars = new char[] {
        '%', '$', '@'
    };

    public TextInterpreterSnapshot Location;
    public Token[] Children;
    public object? Content;
    public TokenType Type;
    internal TokenFlag Flags;

    public Token(TextInterpreterSnapshot location, TokenType type)
    {
        Type = type;
        Children = Array.Empty<Token>();
        Flags = default;
        Content = null;
        Location = location;
    }

    public static bool IsSymbolToken(string content)
    {
        if (content.Length < 1) return false;
        char firstChar = content[0];

        if (!char.IsLetter(firstChar) && !AllowedFirstSymbolChars.Contains(firstChar))
        {
            return false;
        }

        for (int i = 1; i < content.Length; i++)
        {
            char ch = content[i];
            bool cond = char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == ':';
            if (!cond) return false;
        }

        return true;
    }

    public static bool IsStringToken(string content)
    {
        if (content.Length < 2) return false;
        for (int i = 0; i < content.Length; i++)
        {
            char current = content[i];
            char before = content[Math.Max(0, i - 1)];
            if (i == 0 && current != Token.Ch_StringQuote)
            {
                return false;
            }
            else if (i > 0 && current == Token.Ch_StringQuote && before != '\\')
            {
                return i == content.Length - 1;
            }
        }
        return false;
    }
}

[Flags]
internal enum TokenFlag : byte
{
    None = 0,
    CompileTime = 1 << 0,
}

public enum TokenType
{
    // data types
    String = 1,
    Number = 2,
    Boolean = 3,
    Null = 0,

    // represents an entire token group
    Expression = 64,

    // represents an literal symbol or identifier
    Symbol = 10
}