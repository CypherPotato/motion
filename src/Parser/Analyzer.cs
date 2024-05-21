using Motion.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

class Analyzer
{
    List<SyntaxItem> LintedAtoms { get; set; } = new List<SyntaxItem>();
    TextInterpreter Interpreter { get; set; } = null!;
    string Code { get; set; }

    public SyntaxItem[] GetSyntaxItems() => LintedAtoms.ToArray();

    public Analyzer(string code)
    {
        Code = code;
    }

    public void Lint()
    {
        string sanitized = Sanitizer.SanitizeCode(Code, out _, out var comments);
        LintedAtoms.AddRange(comments);

        Interpreter = new TextInterpreter(sanitized, null);

    readNext:
        Interpreter.SkipIgnoreTokens();

        if (Interpreter.CanRead())
        {
            Analyse();
            goto readNext;
        }
    }

    void Analyse()
    {
        Interpreter.SkipIgnoreTokens();

    readNext:
        TextInterpreterSnapshot nextContentSnapshot = Interpreter.TakeSnapshot(1);
        char hit = Interpreter.ReadUntil(new char[] { ' ', '\t', '\r', '\n', AtomBase.Ch_ExpressionStart, AtomBase.Ch_ExpressionEnd }, true, out string content);


        if (hit == AtomBase.Ch_ExpressionStart)
            LintedAtoms.Add(new SyntaxItem(hit.ToString(), SyntaxItemType.ExpressionStart, nextContentSnapshot));

        if (hit == AtomBase.Ch_ExpressionEnd)
            LintedAtoms.Add(new SyntaxItem(hit.ToString(), SyntaxItemType.ExpressionEnd, nextContentSnapshot));

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
        SyntaxItemType type = default;
        snapshot.Length = content.Length;

        if (string.Compare(content, "true", true) == 0 ||
            string.Compare(content, "false", true) == 0)
        {
            type = SyntaxItemType.BooleanLiteral;
        }
        else if (string.Compare(content, "nil", true) == 0)
        {
            type = SyntaxItemType.NullWord;
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
            type = SyntaxItemType.Operator;
        }
        else if (AtomBase.IsStringToken(content))
        {
            type = SyntaxItemType.StringLiteral;
        }
        else if (AtomBase.IsSymbolToken(content))
        {
            type = SyntaxItemType.Symbol;
        }
        else if (AtomBase.IsKeywordToken(content))
        {
            type = SyntaxItemType.Keyword;
        }
        else if (AtomBase.IsNumberToken(content))
        {
            type = SyntaxItemType.NumberLiteral;
        }
        else
        {
            return;
        }

        LintedAtoms.Add(new SyntaxItem(content, type, snapshot));
    }
}
