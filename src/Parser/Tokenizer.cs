using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

class Tokenizer
{
    private string? lastKeyword = null;
    private TextInterpreter Interpreter { get; set; }
    private MotionCompilerOptions CompilerOptions { get; set; }

    public Tokenizer(string code, MotionCompilerOptions options)
    {
        string sanitized = Sanitizer.SanitizeCode(code, out _);
        Interpreter = new TextInterpreter(sanitized);
        CompilerOptions = options;
    }

    public IEnumerable<AtomBase> Tokenize()
    {
    readNext:
        Interpreter.SkipIgnoreTokens();

        if (Interpreter.CanRead())
        {
            yield return TokenizeExpression(0);
            goto readNext;
        }
    }

    AtomBase TokenizeExpression(int depth)
    {
        TextInterpreterSnapshot expStartSnapshot = Interpreter.TakeSnapshot(1);
        List<AtomBase> subTokens = new List<AtomBase>();
        List<string> emptyKeywords = new List<string>();
        AtomBase expression = new AtomBase(expStartSnapshot, TokenType.Expression);

        Interpreter.Read(out char c);

        if (c != AtomBase.Ch_ExpressionStart)
        {
            if (!CompilerOptions.AllowInlineDeclarations)
            {
                throw new MotionException("expected an atom initialization char '('. got " + c, Interpreter);
            }
            else
            {
                Interpreter.Move(-1);
            }
        }

        Interpreter.SkipIgnoreTokens();

    readNext:
        TextInterpreterSnapshot nextContentSnapshot = Interpreter.TakeSnapshot(1);
        char hit = Interpreter.ReadUntil(new char[] { ' ', '\t', '\r', '\n', AtomBase.Ch_ExpressionStart, AtomBase.Ch_ExpressionEnd }, true, out string content);

        if (char.IsWhiteSpace(hit))
        {
            if (content.Length > 0)
            {
                AtomBase item = TokenizePart(ref nextContentSnapshot, content);
                AddTokenToExpression(subTokens, emptyKeywords, ref item);
            }

            goto readNext;
        }
        else if (hit == AtomBase.Ch_ExpressionStart)
        {
            if (content.Length > 0)
            {
                AtomBase item = TokenizePart(ref nextContentSnapshot, content);
                AddTokenToExpression(subTokens, emptyKeywords, ref item);
            }

            Interpreter.Move(-1);
            AtomBase subExpression = TokenizeExpression(depth + 1);
            subTokens.Add(subExpression);
            goto readNext;
        }
        else if (hit == AtomBase.Ch_ExpressionEnd)
        {
            expStartSnapshot.Length = Interpreter.Position - expStartSnapshot.Position;
            expression.Location = expStartSnapshot;
            if (content.Length > 0)
            {
                AtomBase item = TokenizePart(ref nextContentSnapshot, content);
                AddTokenToExpression(subTokens, emptyKeywords, ref item);
            }

            goto finish;
        }
        else if (hit == '\0')
        {
            if (depth == 0 && CompilerOptions.AllowInlineDeclarations)
            {
                expStartSnapshot.Length = Interpreter.Position - expStartSnapshot.Position;
                expression.Location = expStartSnapshot;
                if (content.Length > 0)
                {
                    AtomBase item = TokenizePart(ref nextContentSnapshot, content);
                    AddTokenToExpression(subTokens, emptyKeywords, ref item);
                }

                goto finish;
            }
            else
            {
                throw new MotionException("unclosed expression block.", expStartSnapshot, null);
            }
        }
        else
        {
            throw new MotionException("unexpected token " + content, Interpreter);
        }

    finish:
        expression.SingleKeywords = emptyKeywords.ToArray();
        expression.Children = subTokens.ToArray();
        return expression;
    }

    void AddTokenToExpression(List<AtomBase> creatingExpArr, List<string> keywords, ref AtomBase item)
    {
        // keywords aren't added directly into expressions
        if (item.Type == TokenType.Keyword)
        {
            keywords.Add(item.Content!.ToString()!.Substring(1));

            // does not add keywords into token childrens
            return;
        }

        if (lastKeyword != null)
        {
            item.Keyword = lastKeyword;
            lastKeyword = null;
        }
        creatingExpArr.Add(item);
    }

    AtomBase TokenizePart(ref TextInterpreterSnapshot snapshot, string content)
    {
        try
        {
            snapshot.Length = content.Length;
            if (string.Compare(content, "true", true) == 0)
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
            else if (content == "=" ||
                     content == "/=" ||
                     content == ">" ||
                     content == "<" ||
                     content == ">=" ||
                     content == "<=" ||
                     content == "+" ||
                     content == "-" ||
                     content == "*" ||
                     content == "/"
                    )
            {
                return new AtomBase(snapshot, TokenType.Operator)
                {
                    Content = content
                };
            }
            else if (AtomBase.IsStringToken(content))
            {
                return new AtomBase(snapshot, TokenType.String)
                {
                    Content = Sanitizer.SanitizeStringLiteral(content)
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
                lastKeyword = content.Substring(1);
                return new AtomBase(snapshot, TokenType.Keyword)
                {
                    Content = content
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
                throw new MotionException("unrecognized token: " + content, snapshot, null);
            }
        }
        catch (Exception ex)
        {
            throw new MotionException(ex.Message, snapshot, ex);
        }
    }
}

