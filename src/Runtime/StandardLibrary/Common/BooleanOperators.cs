using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class BooleanOperators : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Aliases.Add("eq", "=");
        context.Aliases.Add("neq", "/=");

        context.Methods.Add("=", Eq);
        context.Methods.Add("/=", Neq);
        context.Methods.Add("and", And);
        context.Methods.Add("or", Or);
        context.Methods.Add("xor", Xor);
        context.Methods.Add("not", Not);
        context.Methods.Add("if", If);
        context.Methods.Add("if-not", IfNot);

        context.Methods.Add("<", (dynamic A, dynamic B) => A < B);
        context.Methods.Add("<=", (dynamic A, dynamic B) => A <= B);
        context.Methods.Add(">", (dynamic A, dynamic B) => A > B);
        context.Methods.Add(">=", (dynamic A, dynamic B) => A >= B);

        context.Methods.Add("zerop", Zerop);
        context.Methods.Add("is-null", IsNull);
        context.Methods.Add("is-not-null", IsNotNull);
    }

    bool Eq(object? a, object? b)
    {
        return a?.Equals(b) == true;
    }

    bool Neq(object? a, object? b)
    {
        return a?.Equals(b) == false;
    }

    bool And(Atom self)
    {
        bool a = self.GetAtom(1).GetBoolean();
        if (!a) return false;
        if (self.GetAtom(2).GetBoolean() == true) return true;
        return false;
    }

    bool Or(Atom self)
    {
        bool a = self.GetAtom(1).GetBoolean();
        if (a)
        {
            return true;
        }
        else if (self.GetAtom(2).GetBoolean() == true) return true;
        return false;
    }

    bool Xor(bool a, bool b)
    {
        return a ^ b;
    }

    bool Not(bool a)
    {
        return !a;
    }

    bool Zerop(dynamic a)
    {
        return a == 0;
    }

    bool IsNull(object? b)
    {
        return b is null;
    }

    bool IsNotNull(object? b)
    {
        return b is not null;
    }

    object? IfNot(Atom self)
    {
        self.EnsureExactItemCount(3, 4);
        bool condition = self.GetAtom(1).GetBoolean();
        if (self.ItemCount == 3)
        {
            if (!condition)
            {
                return self.GetAtom(2).Nullable()?.GetObject();
            }
            else
            {
                return null;
            }
        }
        else if (self.ItemCount == 4)
        {
            if (!condition)
            {
                return self.GetAtom(2).Nullable()?.GetObject();
            }
            else
            {
                return self.GetAtom(3).Nullable()?.GetObject();
            }
        }
        else
        {
            return null;
        }
    }

    object? If(Atom self)
    {
        self.EnsureExactItemCount(3, 4);
        bool condition = self.GetAtom(1).GetBoolean();
        if (self.ItemCount == 3)
        {
            if (condition)
            {
                return self.GetAtom(2).Nullable()?.GetObject();
            }
            else
            {
                return null;
            }
        }
        else if (self.ItemCount == 4)
        {
            if (condition)
            {
                return self.GetAtom(2).Nullable()?.GetObject();
            }
            else
            {
                return self.GetAtom(3).Nullable()?.GetObject();
            }
        }
        else
        {
            return null;
        }
    }
}
