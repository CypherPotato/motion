using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdString : IMotionLibrary
{
    public string? Namespace => "str";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Aliases.Add("concat", "str:concat");
        ;
        context.Methods.Add("concat", Concat);
        context.Methods.Add("make-upper-case", MkUpperCase);
        context.Methods.Add("make-lower-case", MkLowerCase);
        context.Methods.Add("normalize", Normalize);
        context.Methods.Add("trim", Trim);
        context.Methods.Add("trim-end", TrimEnd);
        context.Methods.Add("trim-start", TrimStart);
        context.Methods.Add("explode", Explode);
        context.Methods.Add("implode", Implode);
        context.Methods.Add("pad-left", PadLeft);
        context.Methods.Add("pad-right", PadRight);
        context.Methods.Add("cmp", Cmp);
        context.Methods.Add("contains", Contains);
        context.Methods.Add("ends-with", EndsWith);
        context.Methods.Add("starts-with", StartsWith);
        context.Methods.Add("len", Len);
        context.Methods.Add("index-of", IndexOf);
        context.Methods.Add("last-index-of", LastIndexOf);
        context.Methods.Add("coalesce", Coalesce);
        context.Methods.Add("repeat", Repeat);
        context.Methods.Add("substr", Substr);
    }

    string Concat(params object?[] items) => string.Join("", items);
    string? MkUpperCase(string? s) => s?.ToUpper();
    string? MkLowerCase(string? s) => s?.ToLower();
    string? Normalize(string? s) => s?.Normalize();
    string? Trim(string? s) => s?.Trim();
    string? TrimEnd(string? s) => s?.TrimEnd();
    string? TrimStart(string? s) => s?.TrimStart();
    string[] Explode(string sep, string s) => s.Split(sep);
    string Implode(string sep, params object?[] values) => string.Join(sep, values);
    string PadLeft(string s, char pad, int count) => s.PadLeft(count, pad);
    string PadRight(string s, char pad, int count) => s.PadRight(count, pad);
    int Cmp(Atom self, string? a, string? b) => string.Compare(a, b, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);
    bool Contains(Atom self, string a, string b) => a.Contains(b, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);
    bool EndsWith(Atom self, string a, string b) => a.EndsWith(b, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);
    bool StartsWith(Atom self, string a, string b) => a.StartsWith(b, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);
    int Len(string? s) => s?.Length ?? 0;
    int IndexOf(Atom self, string s, string term) => s.IndexOf(term, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);
    int LastIndexOf(Atom self, string s, string term) => s.LastIndexOf(term, self.HasKeyword("ignore-case") ? StringComparison.CurrentCultureIgnoreCase : StringComparison.Ordinal);

    string? Coalesce(params object?[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            string? s = values[i]?.ToString();
            if (!string.IsNullOrEmpty(s))
            {
                return s;
            }
        }
        return null;
    }

    string Repeat(string? s, int count)
    {
        if (s is null) return "";
        StringBuilder sb = new StringBuilder(s.Length * count);

        for (int i = 0; i < count; i++)
            sb.Append(s);

        return sb.ToString();
    }

    string Substr(Atom self)
    {
        string s = self.GetAtom(1).GetString();
        if (self.ItemCount == 3)
        {
            int startPos = self.GetAtom(2).GetInt32();
            int n = Math.Abs(startPos);

            if (n > s.Length)
                return "";

            if (startPos < 0)
            {
                return s.Substring(s.Length - n);
            }
            else
                return s.Substring(startPos);
        }
        else if (self.ItemCount == 4)
        {
            int startPos = self.GetAtom(2).GetInt32();
            int length = self.GetAtom(3).GetInt32();
            return s.Substring(startPos, length);
        }

        throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
    }
}
