using System.Globalization;
using System.Text;

namespace Motion.Parser;

class Sanitizer
{
    static int Max(int val1, int val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    public static object SanitizeNumberLiteral(string numberLiteral)
    {
        string s = numberLiteral.Replace("_", "");
        bool hasDot = s.Contains('.');

        if (s.EndsWith("F", StringComparison.InvariantCultureIgnoreCase))
        {
            return float.Parse(s[0..(s.Length - 1)], CultureInfo.InvariantCulture);
        }
        else if (s.EndsWith("M", StringComparison.InvariantCultureIgnoreCase))
        {
            return decimal.Parse(s[0..(s.Length - 1)], CultureInfo.InvariantCulture);
        }
        else if (s.EndsWith("L", StringComparison.InvariantCultureIgnoreCase))
        {
            if (hasDot)
            {
                throw new FormatException("Cannot use format 'L' on decimal numbers.");
            }
            else
            {
                return long.Parse(s[0..(s.Length - 1)], CultureInfo.InvariantCulture);
            }
        }
        else if (hasDot)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
        else
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }
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

    public static string SanitizeCode(string input, out int parenthesisIndex)
    {
        StringBuilder output = new StringBuilder();

        char[] inputChars = input.ToCharArray();

        int _parenthesisIndex = 0;
        bool inString = false;
        bool inComment = false;
        for (int i = 0; i < inputChars.Length; i++)
        {
            char current = inputChars[i];
            char before = inputChars[Max(0, i - 1)];

            if (current == '"' && before != '\\') inString = !inString;
            if (current == ';' && !inString) inComment = true;
            if (inComment)
            {
                if (current == '\n' || current == '\r')
                {
                    inComment = false;
                    output.Append(current);
                }
                else
                {
                    output.Append(' ');
                }
                continue;
            }
            if (!inString)
            {
                if (current == '(')
                {
                    _parenthesisIndex++;
                }
                else if (current == ')')
                {
                    _parenthesisIndex--;
                }
            }
            output.Append(current);
        }

        parenthesisIndex = _parenthesisIndex;

        return output.ToString();
    }
}
