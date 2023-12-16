using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Compiler;

internal class Tokenizer
{
    private TextInterpreter Interpreter { get; set; }

    public Tokenizer(string code)
    {
        string sanitized = Sanitizer.SanitizeCode(code);
        Interpreter = new TextInterpreter(sanitized);
    }

    public IEnumerable<Token> Tokenize()
    {
    readNext:
        Interpreter.SkipIgnoreTokens();

        if (Interpreter.CanRead())
        {
            yield return TokenizeExpression();
            goto readNext;
        }
    }

    Token TokenizeExpression()
    {
        TextInterpreterSnapshot expStartSnapshot = Interpreter.TakeSnapshot(1);
        List<Token> subTokens = new List<Token>();
        Token expression = new Token(expStartSnapshot, TokenType.Expression);

        Interpreter.Read(out char c);

        if (c != Token.Ch_ExpressionStart)
        {
            throw new MotionException("unexpected char: " + c, Interpreter);
        }

        Interpreter.SkipIgnoreTokens();

    readNext:
        TextInterpreterSnapshot nextContentSnapshot = Interpreter.TakeSnapshot(1);
        char hit = Interpreter.ReadUntil(new char[] { ' ', '\t', '\r', '\n', Token.Ch_ExpressionStart, Token.Ch_ExpressionEnd }, true, out string content);

        if (char.IsWhiteSpace(hit))
        {
            if (content.Length > 0)
            {
                Token item = TokenizePart(ref nextContentSnapshot, content);

                if (subTokens.Count == 0 && item.Type != TokenType.Symbol)
                {
                    throw new MotionException($"method name expected. got {item.Type} instead.", Interpreter);
                }

                subTokens.Add(item);
            }
            goto readNext;
        }
        else if (hit == Token.Ch_ExpressionStart)
        {
            if (content.Length > 0)
            {
                Token item = TokenizePart(ref nextContentSnapshot, content);

                if (subTokens.Count == 0 && item.Type != TokenType.Symbol)
                {
                    throw new MotionException($"method name expected. got {item.Type} instead.", Interpreter);
                }

                subTokens.Add(item);
            }
            Interpreter.Move(-1);
            Token subExpression = TokenizeExpression();
            subTokens.Add(subExpression);
            goto readNext;
        }
        else if (hit == Token.Ch_ExpressionEnd)
        {
            expStartSnapshot.Length = Interpreter.Position - expStartSnapshot.Position;
            expression.Location = expStartSnapshot;
            if (content.Length > 0)
            {
                Token item = TokenizePart(ref nextContentSnapshot, content);
                subTokens.Add(item);
            }
            goto finish;
        }
        else if (hit == '\0')
        {
            throw new MotionException("unclosed expression block.", expStartSnapshot, null);
        }
        else
        {
            throw new MotionException("unexpected char " + hit, Interpreter);
        }

    finish:
            
        expression.Children = subTokens.ToArray();    
        return expression;
    }

    Token TokenizePart(ref TextInterpreterSnapshot snapshot, string content)
    {
        snapshot.Length = content.Length;
        if (content == "true")
        {
            return new Token(snapshot, TokenType.Boolean)
            {
                Content = true
            };
        }
        else if (content == "false")
        {
            return new Token(snapshot, TokenType.Boolean)
            {
                Content = false
            };
        }
        else if (content == "nothing")
        {
            return new Token(snapshot, TokenType.Null)
            {
                Content = null
            };
        }
        else if (Token.IsStringToken(content))
        {
            return new Token(snapshot, TokenType.String)
            {
                Content = Sanitizer.SanitizeStringLiteral(content)
            };
        }
        else if (Token.IsSymbolToken(content))
        {
            return new Token(snapshot, TokenType.Symbol)
            {
                Content = content
            };
        }
        else if (double.TryParse(content, out double result))
        {
            return new Token(snapshot, TokenType.Number)
            {
                Content = result
            };
        }
        else
        {
            throw new MotionException("unrecognized token: " + content, snapshot, null);
        }
    }
}
