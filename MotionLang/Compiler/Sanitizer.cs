using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Compiler;
internal class Sanitizer
{
    static int Max(int val1, int val2)
    {
        return (val1 >= val2) ? val1 : val2;
    }

    public static string SanitizeStringLiteral(string stringLiteral)
    {
        bool verbatin = false;
        if (stringLiteral.StartsWith('^'))
        {
            verbatin = true;
            stringLiteral = stringLiteral.Substring(1);
        }
        string S = stringLiteral.Substring(1, stringLiteral.Length - 2);
        if (!verbatin)
        {
            S = S.Replace(@"\\", "\\");
            S = S.Replace(@"\n", "\n");
            S = S.Replace(@"\r", "\r");
            S = S.Replace(@"\t", "\t");
        }
        S = S.Replace(@"\""", "\"");
        return S;
    }

    public static string SanitizeCode(string input)
    {
        StringBuilder output = new StringBuilder();

        char[] inputChars = input.ToCharArray();

        bool inString = false;
        bool inComment = false;
        for (int i = 0; i < inputChars.Length; i++)
        {
            char current = inputChars[i];
            char before = inputChars[Max(0, i - 1)];

            if (current == '"' && before != '\\') inString = !inString;
            if (current == ';' && !inString) inComment = true;
            if (inComment && (current == '\n' || current == '\r')) inComment = false;
            if (!inComment) output.Append(current);
        }

        return output.ToString().Trim();
    }
}
