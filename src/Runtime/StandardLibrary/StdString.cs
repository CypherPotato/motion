using System;
using System.Collections;
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
        context.Methods.Add("concat", (atom) =>
        {
            StringBuilder result = new StringBuilder();
            foreach (Atom t in atom.GetAtoms().Skip(1))
            {
                result.Append(t.Nullable()?.GetString());
            }
            return result.ToString();
        });
        context.Methods.Add("repeat", (atom) =>
        {
            string? s = atom.GetAtom(1).Nullable()?.GetString();
            int count = atom.GetAtom(2).GetInt32();

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(s);
            }

            return result.ToString();
        });
        context.Methods.Add("make-upper-case", (atom) => atom.GetAtom(1).Nullable()?.GetString().ToUpper());
        context.Methods.Add("make-lower-case", (atom) => atom.GetAtom(1).Nullable()?.GetString().ToLower());
        context.Methods.Add("normalize", (atom) => atom.GetAtom(1).Nullable()?.GetString().Normalize());
        context.Methods.Add("trim", (atom) => atom.GetAtom(1).Nullable()?.GetString().Trim());
        context.Methods.Add("trim-end", (atom) => atom.GetAtom(1).Nullable()?.GetString().TrimEnd());
        context.Methods.Add("trim-start", (atom) => atom.GetAtom(1).Nullable()?.GetString().TrimStart());
        context.Methods.Add("len", (atom) => atom.GetAtom(1).Nullable()?.GetString().Length);
        context.Methods.Add("substr", (atom) =>
        {
            string s = atom.GetAtom(1).GetString();
            if (atom.ItemCount == 3)
            {
                int startPos = atom.GetAtom(2).GetInt32();
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
            else if (atom.ItemCount == 4)
            {
                int startPos = atom.GetAtom(2).GetInt32();
                int length = atom.GetAtom(3).GetInt32();
                return s.Substring(startPos, length);
            }

            throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
        });
        context.Methods.Add("icmp", (atom) =>
        {
            string a = atom.GetAtom(1).GetString();
            string? b = atom.GetAtom(2).Nullable()?.GetObject().ToString();

            return string.Compare(a, b, true) == 0;
        });
        context.Methods.Add("icontains", (atom) =>
        {
            string a = atom.GetAtom(1).GetString();
            string? b = atom.GetAtom(2).Nullable()?.GetObject().ToString();

            if (b == null)
                return false;

            return a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
        });
        context.Methods.Add("index-of", (atom) =>
        {
            string s = atom.GetAtom(1).GetString();
            if (atom.ItemCount == 3)
            {
                string S = atom.GetAtom(2).GetString();
                return s.IndexOf(S);
            }
            else if (atom.ItemCount == 4)
            {
                string S = atom.GetAtom(2).GetString();
                int startPos = atom.GetAtom(3).GetInt32();
                return s.IndexOf(S, startPos);
            }

            throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
        });
        context.Methods.Add("last-index-of", (atom) =>
        {
            string s = atom.GetAtom(1).GetString();
            if (atom.ItemCount == 3)
            {
                string S = atom.GetAtom(2).GetString();
                return s.LastIndexOf(S);
            }
            else if (atom.ItemCount == 4)
            {
                string S = atom.GetAtom(2).GetString();
                int startPos = atom.GetAtom(3).GetInt32();
                return s.LastIndexOf(S, startPos);
            }

            throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
        });
        context.Methods.Add("fmt", (atom) =>
        {
            object?[] args = atom.GetAtoms()
                .Skip(2)
                .Select(a => a.GetObject())
                .ToArray();

            return string.Format(atom.GetAtom(1).GetString(), args);
        });
        context.Methods.Add("explode", (atom) =>
        {
            string s = atom.GetAtom(1).GetString();

            if (atom.ItemCount == 2)
            {
                return s.ToCharArray();
            }
            else if (atom.ItemCount == 3)
            {
                string separator = atom.GetAtom(2).GetString();
                return s.Split(separator);
            }

            throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
        });
        context.Methods.Add("implode", (atom) =>
        {
            if (atom.ItemCount == 2)
            {
                IEnumerable items = (IEnumerable)atom.GetAtom(1).GetObject();
                StringBuilder sb = new StringBuilder();

                foreach (object? s in items)
                    sb.Append(s);

                return sb.ToString();
            }
            else if (atom.ItemCount == 3)
            {
                IEnumerable items = (IEnumerable)atom.GetAtom(2).GetObject();
                string separator = atom.GetAtom(1).GetString();
                StringBuilder sb = new StringBuilder();

                foreach (object? s in items)
                {
                    if (s != null)
                    {
                        sb.Append(s);
                        sb.Append(separator);
                    }
                }

                sb.Length -= separator.Length;
                return sb.ToString();
            }

            throw new InvalidOperationException($"this method expects only 2 or 3 parameters.");
        });
        context.Methods.Add("coalesce", (atom) =>
        {
            for (int i = 1; i < atom.ItemCount; i++)
            {
                object? data = atom.GetAtom(i).Nullable()?.GetObject();
                if (!string.IsNullOrEmpty(data?.ToString()))
                {
                    return data;
                }
            }
            return null;
        });
        context.Methods.Add("pad-left", (atom) =>
        {
            atom.EnsureExactItemCount(4);

            string s = atom.GetAtom(1).GetString();
            char pad = atom.GetAtom(2).GetChar();
            int count = atom.GetAtom(3).GetInt32();

            return s.PadLeft(count, pad);
        });
        context.Methods.Add("pad-right", (atom) =>
        {
            atom.EnsureExactItemCount(4);

            string s = atom.GetAtom(1).GetString();
            char pad = atom.GetAtom(2).GetChar();
            int count = atom.GetAtom(3).GetInt32();

            return s.PadRight(count, pad);
        });
    }
}
