using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

class Linter
{
    List<AtomBase> LintedAtoms { get; set; } = new List<AtomBase>();
    TextInterpreter Interpreter { get; set; }

    public Linter(string code)
    {
        string sanitized = Sanitizer.SanitizeCode(code, out _);
        Interpreter = new TextInterpreter(sanitized, null);
    }

    public AtomBase[] Lint()
    {
    readNext:
        Interpreter.SkipIgnoreTokens();

        if (Interpreter.CanRead())
        {
            Analyse();
            goto readNext;
        }

        return LintedAtoms.ToArray();
    }

    void Analyse()
    {
        Interpreter.SkipIgnoreTokens();

    readNext:
        TextInterpreterSnapshot nextContentSnapshot = Interpreter.TakeSnapshot(1);
        char hit = Interpreter.ReadUntil(new char[] { ' ', '\t', '\r', '\n', AtomBase.Ch_ExpressionStart, AtomBase.Ch_ExpressionEnd }, true, out string content);

        TokenizePart(ref nextContentSnapshot, content);

        if (hit == '\0')
        {
            return;
        }
        else
        {
            goto readNext;
        }
    }

    void TokenizePart(ref TextInterpreterSnapshot snapshot, string content)
    {
        try
        {
            snapshot.Length = content.Length;
            if (string.Compare(content, "true", true) == 0)
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Boolean));
            }
            else if (string.Compare(content, "false", true) == 0)
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Boolean));
            }
            else if (string.Compare(content, "nil", true) == 0)
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Null));
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
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Operator));
            }
            else if (AtomBase.IsStringToken(content))
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.String));
            }
            else if (AtomBase.IsSymbolToken(content))
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Symbol));
            }
            else if (AtomBase.IsKeywordToken(content))
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Keyword));
            }
            else if (AtomBase.IsNumberToken(content))
            {
                LintedAtoms.Add(new AtomBase(snapshot, TokenType.Number));
            }
            else
            {
                ;
            }
        }
        catch
        {
            ;
        }
    }
}
