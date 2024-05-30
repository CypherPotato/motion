using Motion.Compilation;
using Motion.Parser.V2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Motion.Parser;

class Tokenizer : IDisposable
{
    private TextInterpreter interpreter;
    private List<AtomBase> atoms;
    private CompilerOptions options;

    public AtomBase[] Result { get => atoms.ToArray(); }

    public Tokenizer(CompilerSource source, CompilerOptions options)
    {
        interpreter = new TextInterpreter(source.Filename, source.Source);
        atoms = new List<AtomBase>();
        this.options = options;
    }

    public void Dispose()
    {
        interpreter.Dispose();
    }

    public void Read()
    {
        while (interpreter.CanRead)
        {
            interpreter.SkipWhitespace();

            char p = interpreter.Peek();
            switch (p)
            {
                case AtomBase.Ch_ExpressionStart:
                    atoms.Add(ReadExpression());
                    break;

                case AtomBase.Ch_CommentChar:
                    interpreter.SkipComment();
                    break;

                default:
                    if (options.Features.HasFlag(CompilerFeature.AllowParenthesislessCode))
                    {
                        atoms.Add(ReadParenthesislessExpression());
                    }
                    else
                    {
                        throw interpreter.ExceptionManager.UnexpectedToken(p.ToString());
                    }
                    break;
            }

            interpreter.SkipWhitespace();
        }
    }

    AtomBase ReadParenthesislessExpression()
    {
        List<AtomBase> child = new List<AtomBase>();
        List<string> keywords = new List<string>();
        string? lastKeyword = null;

        var expStart = interpreter.GetSnapshot(0);

        string? TakeKeyword()
        {
            try
            {
                return lastKeyword;
            }
            finally
            {
                lastKeyword = null;
            }
        }

        while (interpreter.CanRead)
        {
            interpreter.SkipWhitespace();
            var peek = interpreter.Peek();

            switch (peek)
            {
                case AtomBase.Ch_CommentChar:
                    interpreter.SkipComment();
                    break;

                case AtomBase.Ch_ExpressionEnd:
                    throw interpreter.ExceptionManager.UnexpectedToken(AtomBase.Ch_ExpressionEnd.ToString());

                case AtomBase.Ch_ExpressionStart:

                    AtomBase subExp = ReadExpression();
                    subExp.Keyword = TakeKeyword();

                    child.Add(subExp);
                    break;

                case AtomBase.Ch_ArrStart:

                    AtomBase arr = ReadArray();
                    arr.Keyword = TakeKeyword();

                    child.Add(arr);
                    break;

                case AtomBase.Ch_RawStringId:

                    interpreter.Read();
                    if (interpreter.Peek() == AtomBase.Ch_StringQuote)
                    {
                        AtomBase sltr = ReadRawStringLiteral();
                        sltr.Keyword = TakeKeyword();

                        child.Add(sltr);
                    }
                    else
                    {
                        goto default;
                    }

                    break;

                case AtomBase.Ch_StringQuote:
                case AtomBase.Ch_StringVerbatin:

                    AtomBase at = ReadString();
                    at.Keyword = TakeKeyword();

                    child.Add(at);
                    break;

                default:

                    AtomBase c = ReadCarry(true, true);
                    if (c.Type == TokenType.Keyword)
                    {
                        lastKeyword = c.Content?.ToString();
                        keywords.Add(lastKeyword!);
                    }
                    else
                    {
                        c.Keyword = TakeKeyword();
                        child.Add(c);
                    }

                    break;
            }

            interpreter.SkipWhitespace();
        }

        return new AtomBase(expStart, TokenType.Expression)
        {
            SingleKeywords = keywords.ToArray(),
            Children = child.ToArray()
        };
    }

    AtomBase ReadExpression()
    {
        List<AtomBase> child = new List<AtomBase>();
        List<string> keywords = new List<string>();
        string? lastKeyword = null;

        interpreter.SkipWhitespace();

        var expStart = interpreter.GetSnapshot(0);
        interpreter.Assert(AtomBase.Ch_ExpressionStart);

        interpreter.SkipWhitespace();

        string? TakeKeyword()
        {
            try
            {
                return lastKeyword;
            }
            finally
            {
                lastKeyword = null;
            }
        }

        while (interpreter.CanRead)
        {
            interpreter.SkipWhitespace();
            var peek = interpreter.Peek();

            switch (peek)
            {
                case AtomBase.Ch_CommentChar:
                    interpreter.SkipComment();
                    break;

                case AtomBase.Ch_ExpressionEnd:

                    interpreter.Read();

                    expStart.Length = interpreter.Position.index - expStart.Index;
                    return new AtomBase(expStart, TokenType.Expression)
                    {
                        SingleKeywords = keywords.ToArray(),
                        Children = child.ToArray()
                    };

                case AtomBase.Ch_ExpressionStart:

                    AtomBase subExp = ReadExpression();
                    subExp.Keyword = TakeKeyword();

                    child.Add(subExp);
                    break;

                case AtomBase.Ch_ArrStart:

                    AtomBase arr = ReadArray();
                    arr.Keyword = TakeKeyword();

                    child.Add(arr);
                    break;

                case AtomBase.Ch_RawStringId:

                    interpreter.Read();
                    if (interpreter.Peek() == AtomBase.Ch_StringQuote)
                    {
                        AtomBase sltr = ReadRawStringLiteral();
                        sltr.Keyword = TakeKeyword();

                        child.Add(sltr);
                    }
                    else
                    {
                        goto default;
                    }

                    break;

                case AtomBase.Ch_StringQuote:
                case AtomBase.Ch_StringVerbatin:

                    AtomBase at = ReadString();
                    at.Keyword = TakeKeyword();

                    child.Add(at);
                    break;

                default:

                    AtomBase c = ReadCarry(true, false);

                    if (c.Type == TokenType.Keyword)
                    {
                        lastKeyword = c.Content?.ToString();
                        keywords.Add(lastKeyword!);
                    }
                    else
                    {
                        c.Keyword = TakeKeyword();
                        child.Add(c);
                    }

                    break;
            }

            interpreter.SkipWhitespace();
        }

        throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_ExpressionEnd.ToString());
    }

    AtomBase ReadArray()
    {
        List<AtomBase> child = new List<AtomBase>();

        interpreter.SkipWhitespace();

        var expStart = interpreter.GetSnapshot(0);
        interpreter.Assert(AtomBase.Ch_ArrStart);

        interpreter.SkipWhitespace();

        while (interpreter.CanRead)
        {
            interpreter.SkipWhitespace();
            var peek = interpreter.Peek();

            switch (peek)
            {
                case AtomBase.Ch_CommentChar:
                    interpreter.SkipComment();
                    break;

                case AtomBase.Ch_ExpressionStart:

                    AtomBase subExp = ReadExpression();
                    child.Add(subExp);

                    break;

                case AtomBase.Ch_ArrStart:

                    AtomBase subArr = ReadArray();
                    child.Add(subArr);

                    break;


                case AtomBase.Ch_ArrEnd:

                    interpreter.Read();

                    expStart.Length = interpreter.Position.index - expStart.Index;
                    return new AtomBase(expStart, TokenType.Array)
                    {
                        Children = child.ToArray()
                    };

                case AtomBase.Ch_RawStringId:

                    interpreter.Read();
                    if (interpreter.Peek() == AtomBase.Ch_StringQuote)
                    {
                        AtomBase sltr = ReadRawStringLiteral();
                        child.Add(sltr);
                    }
                    else
                    {
                        goto default;
                    }

                    break;

                case AtomBase.Ch_StringQuote:
                case AtomBase.Ch_StringVerbatin:

                    AtomBase at = ReadString();
                    child.Add(at);
                    break;

                default:

                    AtomBase c = ReadCarry(false, false);

                    if (c.Type == TokenType.Keyword)
                    {
                        throw interpreter.ExceptionManager.UnknownExpression(c.Content!.ToString()!, c.Location);
                    }
                    else
                    {
                        child.Add(c);
                    }

                    break;
            }

            interpreter.SkipWhitespace();
        }

        throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_ArrEnd.ToString());
    }

    AtomBase ReadRawStringLiteral()
    {
        StringBuilder sb = new StringBuilder();

        var start = interpreter.GetSnapshot(1);
        int mkSize = 0,
            carryEndSize = 0;

        while (interpreter.CanRead && interpreter.Peek() == AtomBase.Ch_StringQuote)
        {
            mkSize++;
            interpreter.Read();
        }

        if (mkSize == 0)
        {
            throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_StringQuote.ToString());
        }

        while (interpreter.CanRead)
        {
            var r = interpreter.Read();

            if (r == AtomBase.Ch_StringQuote)
            {
                carryEndSize++;
                if (mkSize == carryEndSize)
                {
                    start.Length = interpreter.Position.index - start.Index;
                    return new AtomBase(start, TokenType.String)
                    {
                        Content = sb.ToString()
                    };
                }
            }
            else
            {
                if (carryEndSize > 0)
                {
                    sb.Append(new string(AtomBase.Ch_StringQuote, carryEndSize));
                    carryEndSize = 0;
                }

                sb.Append(r);
            }
        }

        throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_StringQuote.ToString());
    }

    AtomBase ReadString()
    {
        StringBuilder sb = new StringBuilder();

        var start = interpreter.GetSnapshot(1);

        if (interpreter.Peek() == AtomBase.Ch_StringVerbatin)
        {
            sb.Append(interpreter.Read());
        }

        interpreter.Assert(AtomBase.Ch_StringQuote);

        sb.Append(interpreter.Current);

        while (interpreter.CanRead)
        {
            var peek = interpreter.Peek();

            switch (peek)
            {
                case AtomBase.Ch_StringQuote:

                    char current = interpreter.Current;
                    interpreter.Read();
                    sb.Append(interpreter.Current);

                    if (current != AtomBase.Ch_StringEscape)
                    {
                        start.Length = interpreter.Position.index - start.Index;
                        return new AtomBase(start, TokenType.String)
                        {
                            Content = Sanitizer.SanitizeStringLiteral(sb.ToString())
                        };
                    }
                    break;

                default:
                    sb.Append(interpreter.Read());
                    break;
            }
        }

        throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_StringQuote.ToString());
    }

    AtomBase ReadCarry(bool isExp, bool isOnParenthesislessContext)
    {
        StringBuilder sb = new StringBuilder();
        bool fullyRead = false;

        var snapshot = interpreter.GetSnapshot(1);
        sb.Append(interpreter.Read());

        while (interpreter.CanRead)
        {
            var peek = interpreter.Peek();

            if (TextInterpreter.IsWhiteSpace(peek) ||
                (isExp && peek == AtomBase.Ch_ExpressionEnd) ||
                (!isExp && peek == AtomBase.Ch_ArrEnd))
            {
                fullyRead = true;
                break;
            }
            else if (peek == AtomBase.Ch_CommentChar)
            {
                fullyRead = true;
                interpreter.SkipComment();
                break;
            }
            else
            {
                sb.Append(interpreter.Read());
            }
        }

        string content = sb.ToString();
        snapshot.Length = content.Length;

        if (!fullyRead && !isOnParenthesislessContext)
        {
            throw interpreter.ExceptionManager.ExpectToken(AtomBase.Ch_ExpressionEnd.ToString());
        }

        if (content == "="
         || content == "/="
         || content == ">"
         || content == "<"
         || content == ">="
         || content == "<="
         || content == "+"
         || content == "-"
         || content == "*"
         || content == "/"
                )
        {
            return new AtomBase(snapshot, TokenType.Operator)
            {
                Content = content
            };
        }
        else if (string.Compare(content, "true", true) == 0)
        {
            return new AtomBase(snapshot, TokenType.Boolean)
            {
                Content = true
            };
        }
        else if (string.Compare(content, "false", true) == 0)
        {
            return new AtomBase(snapshot, TokenType.Boolean)
            {
                Content = false
            };
        }
        else if (string.Compare(content, "nil", true) == 0)
        {
            return new AtomBase(snapshot, TokenType.Null)
            {
                Content = null
            };
        }
        else if (AtomBase.IsCharacterToken(content))
        {
            return new AtomBase(snapshot, TokenType.Character)
            {
                Content = Sanitizer.SanitizeCharacterLiteral(content)
            };
        }
        else if (AtomBase.IsSymbolToken(content))
        {
            return new AtomBase(snapshot, TokenType.Symbol)
            {
                Content = content
            };
        }
        else if (AtomBase.IsKeywordToken(content))
        {
            return new AtomBase(snapshot, TokenType.Keyword)
            {
                Content = content.Substring(1)
            };
        }
        else if (AtomBase.IsNumberToken(content))
        {
            return new AtomBase(snapshot, TokenType.Number)
            {
                Content = Sanitizer.SanitizeNumberLiteral(content)
            };
        }
        else
        {
            throw interpreter.ExceptionManager.UnknownExpression(content, snapshot);
        }
    }
}
