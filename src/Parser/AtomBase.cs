using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

struct AtomBase
{
    public const char Ch_ExpressionStart = '(';
    public const char Ch_ExpressionEnd = ')';
    public const char Ch_StringQuote = '"';
    public const char Ch_StringVerbatin = '^';
    public const char Ch_StringEscape = '\\';
    public const char Ch_CommentChar = ';';

    public static readonly char[] AllowedFirstSymbolChars = new char[] {
        '%', '$'
    };

    public static readonly AtomBase Undefined = new AtomBase(default, TokenType.Undefined);

    public TextInterpreterSnapshot Location;
    public AtomBase[] Children;
    public string[] SingleKeywords;
    public object? Content;
    public TokenType Type;
    public string? Keyword;

    public override string ToString()
    {
        if (Children.Length > 0)
        {
            return $"({Children[0].Content} [...{Children.Length - 1}])";
        }
        else if (Type == TokenType.Null)
        {
            return "NIL";
        }
        else if (Type == TokenType.Operator)
        {
            return "Op. " + Content?.ToString();
        }
        else if (Type == TokenType.String)
        {
            return $"\"{Content}\"";
        }
        else
        {
            return Content?.ToString() ?? "";
        }
    }

    public AtomBase(TextInterpreterSnapshot location, TokenType type)
    {
        Type = type;
        Children = Array.Empty<AtomBase>();
        SingleKeywords = Array.Empty<string>();
        Content = null;
        Location = location;
    }

    public static bool IsKeywordToken(string content)
    {
        if (content.Length < 1) return false;
        char firstChar = content[0];

        if (firstChar != ':')
        {
            return false;
        }

        for (int i = 1; i < content.Length; i++)
        {
            char ch = content[i];
            bool cond = char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '*';
            if (!cond) return false;
        }

        return true;
    }

    public static bool IsSymbolToken(string content)
    {
        if (content.Length < 1) return false;
        char firstChar = content[0];

        if (!char.IsLetter(firstChar) && !AllowedFirstSymbolChars.Contains(firstChar))
        {
            return false;
        }

        int doubleDotOcurrences = 0;

        for (int i = 1; i < content.Length; i++)
        {
            char ch = content[i];
            bool cond = char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == ':' || ch == '.';

            if (ch == ':')
            {
                if (doubleDotOcurrences == 0)
                {
                    doubleDotOcurrences++;
                }
                else
                {
                    return false;
                }
            }

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
            if (i == 0 && current != Ch_StringQuote)
            {
                return false;
            }
            else if (i > 0 && current == Ch_StringQuote && before != '\\')
            {
                return i == content.Length - 1;
            }
        }
        return false;
    }

    public static bool IsNumberToken(string content)
    {
        if (content.Length == 0) return false;
        char first = content[0];
        char last = content[^1];

        bool hadDot = false;
        bool condFirstChar = char.IsDigit(first) || first == '-' || first == '+';
        bool condLastChar = char.IsDigit(last) || char.ToLowerInvariant(last) == 'f' || char.ToLowerInvariant(last) == 'm' || char.ToLowerInvariant(last) == 'l';

        if (!condFirstChar) return false;
        if (!condLastChar) return false;

        for (int i = 1; i < content.Length - 1; i++)
        {
            char ch = content[i];
            bool condA = char.IsDigit(ch) || ch == '_';
            bool condB = ch == '.';

            if (condB && hadDot)
            {
                return false;
            }
            else if (condB)
            {
                hadDot = true;
            }
            else if (!condA)
            {
                return false;
            }
        }

        return true;
    }

    public AtomBase? ChildrenAt(int index)
    {
        if (Type != TokenType.Expression) return null;
        if (Children.Length < index) return null;
        return Children[index];
    }

    public bool ContentStrCmp(string content)
    {
        return Content != null && Content.ToString() == content;
    }
}

internal enum TokenType
{
    // default types
    Undefined = 0,
    Null = 1,

    // data types
    String = 10,
    Number = 11,
    Boolean = 12,

    // represents an entire token group
    Expression = 40,

    // represents an literal symbol or identifier
    Symbol = 70,
    Keyword = 71,
    Operator = 73
}