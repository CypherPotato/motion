using Motion.Compilation;
using Motion.Parser.V2;
using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

/// <summary>
/// Provides an safe tokenizer which can analyze and classify an Motion input.
/// </summary>
public sealed class SyntaxClassifier : IDisposable
{
    private TextInterpreter interpreter;
    private List<SyntaxItem> items;
    private int expressionDepth = 0;

    /// <summary>
    /// Creates an new instance of the <see cref="SyntaxClassifier"/> class with specified <see cref="CompilerSource"/>.
    /// </summary>
    /// <param name="source">The code input.</param>
    public SyntaxClassifier(CompilerSource source)
    {
        interpreter = new TextInterpreter(null, source.Source);
        items = new List<SyntaxItem>();
    }

    /// <summary>
    /// Get the classified result.
    /// </summary>
    public SyntaxItem[] Result { get => items.ToArray(); }

    /// <summary>
    /// Reads the code and classifies the input tokens.
    /// </summary>
    public void Classify()
    {
        items.Clear();

        while (interpreter.CanRead)
        {
            interpreter.SkipWhitespace();

            char p = interpreter.Peek();
            switch (p)
            {
                case AtomBase.Ch_ExpressionStart:
                    items.Add(new SyntaxItem(p.ToString(), SyntaxItemType.ExpressionStart, expressionDepth, interpreter.GetSnapshot(1)));
                    interpreter.Read();
                    expressionDepth++;
                    break;

                case AtomBase.Ch_ExpressionEnd:
                    expressionDepth--;
                    items.Add(new SyntaxItem(p.ToString(), SyntaxItemType.ExpressionEnd, expressionDepth, interpreter.GetSnapshot(1)));
                    interpreter.Read();
                    break;

                case AtomBase.Ch_ArrStart:
                    items.Add(new SyntaxItem(p.ToString(), SyntaxItemType.ArrayStart, expressionDepth, interpreter.GetSnapshot(1)));
                    interpreter.Read();
                    expressionDepth++;
                    break;

                case AtomBase.Ch_ArrEnd:
                    expressionDepth--;
                    items.Add(new SyntaxItem(p.ToString(), SyntaxItemType.ArrayEnd, expressionDepth, interpreter.GetSnapshot(1)));
                    interpreter.Read();
                    break;

                case AtomBase.Ch_StringQuote:
                case AtomBase.Ch_StringVerbatin:
                    items.Add(ReadString());
                    break;

                case AtomBase.Ch_RawStringId:

                    var start = interpreter.GetSnapshot(1);
                    interpreter.Read();
                    if (interpreter.Peek() == AtomBase.Ch_StringQuote)
                    {
                        items.Add(ReadRawStringLiteral(ref start));
                    }
                    else
                    {
                        goto default;
                    }

                    break;

                case AtomBase.Ch_CommentChar:
                    ReadSkipComment();
                    break;

                default:
                    items.Add(ReadCarry());
                    break;
            }

            interpreter.SkipWhitespace();
        }
    }

    void ReadSkipComment()
    {
        var peek = interpreter.Peek();

        if (peek == AtomBase.Ch_CommentChar)
        {
            var snapshot = interpreter.GetSnapshot(1);
            StringBuilder sb = new StringBuilder();

            while (interpreter.CanRead)
            {
                peek = interpreter.Peek();
                if (TextInterpreter.IsNewLine(peek))
                {
                    break;
                }
                else
                {
                    sb.Append(interpreter.Read());
                }
            }

            if (sb.Length > 0)
            {
                snapshot.Length = sb.Length;
                items.Add(new SyntaxItem(sb.ToString(), SyntaxItemType.Comment, expressionDepth, snapshot));
            }
        }
    }

    SyntaxItem ReadRawStringLiteral(ref TextInterpreterSnapshot start)
    {
        StringBuilder sb = new StringBuilder();

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
                    break;
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

        start.Length = interpreter.Position.index - start.Index;
        return new SyntaxItem(sb.ToString(), SyntaxItemType.RawStringLiteral, expressionDepth, start);
    }

    SyntaxItem ReadString()
    {
        StringBuilder sb = new StringBuilder();
        var start = interpreter.GetSnapshot(1);

        if (interpreter.Peek() == AtomBase.Ch_StringVerbatin)
        {
            if (interpreter.CanRead)
                sb.Append(interpreter.Read());
        }

        if (interpreter.CanRead && interpreter.Read() == AtomBase.Ch_StringQuote)
        {
            ;
        }
        else
        {
            return new SyntaxItem("", SyntaxItemType.Unknown, expressionDepth, start);
        }

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
                        goto breakWhile;
                    }
                    break;

                default:
                    sb.Append(interpreter.Read());
                    break;
            }
        }

    breakWhile:
        start.Length = interpreter.Position.index - start.Index;
        return new SyntaxItem(sb.ToString(), SyntaxItemType.StringLiteral, expressionDepth, start);
    }

    SyntaxItem ReadCarry()
    {
        StringBuilder sb = new StringBuilder();

        var snapshot = interpreter.GetSnapshot(1);
        if (interpreter.CanRead)
            sb.Append(interpreter.Read());

        while (interpreter.CanRead)
        {
            var peek = interpreter.Peek();

            if (TextInterpreter.IsWhiteSpace(peek) ||
                 (peek == AtomBase.Ch_ExpressionEnd) ||
                 (peek == AtomBase.Ch_ArrEnd))
            {
                break;
            }
            else if (peek == AtomBase.Ch_CommentChar)
            {
                ReadSkipComment();
                break;
            }
            else
            {
                sb.Append(interpreter.Read());
            }
        }

        string content = sb.ToString();
        snapshot.Length = content.Length;

        if (content == "="
         || content == "/="
         || content == ">"
         || content == "<"
         || content == ">="
         || content == "<="
         || content == "+"
         || content == "-"
         || content == "*"
         || content == "/")
        {
            return new SyntaxItem(content, SyntaxItemType.Operator, expressionDepth, snapshot);
        }
        else if (string.Compare(content, "true", true) == 0
              || string.Compare(content, "false", true) == 0)
        {
            return new SyntaxItem(content, SyntaxItemType.BooleanLiteral, expressionDepth, snapshot);
        }
        else if (string.Compare(content, "nil", true) == 0)
        {
            return new SyntaxItem(content, SyntaxItemType.NullWord, expressionDepth, snapshot);
        }
        else if (AtomBase.IsCharacterToken(content))
        {
            return new SyntaxItem(content, SyntaxItemType.CharacterLiteral, expressionDepth, snapshot);
        }
        else if (AtomBase.IsSymbolToken(content))
        {
            return new SyntaxItem(content, SyntaxItemType.Symbol, expressionDepth, snapshot);
        }
        else if (AtomBase.IsKeywordToken(content))
        {
            return new SyntaxItem(content, SyntaxItemType.Keyword, expressionDepth, snapshot);
        }
        else if (AtomBase.IsNumberToken(content))
        {
            return new SyntaxItem(content, SyntaxItemType.NumberLiteral, expressionDepth, snapshot);
        }
        else
        {
            return new SyntaxItem(content, SyntaxItemType.Unknown, expressionDepth, snapshot);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        interpreter.Dispose();
    }
}
