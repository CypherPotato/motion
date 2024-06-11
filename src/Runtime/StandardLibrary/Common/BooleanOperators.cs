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

        context.Methods.Add("<", Lt);
        context.Methods.Add("<=", Lte);
        context.Methods.Add(">", Gt);
        context.Methods.Add(">=", Gte);

        context.Methods.Add("zerop", Zerop);
        context.Methods.Add("evenp", Evenp);
        context.Methods.Add("oddp", Oddp);
        context.Methods.Add("minusp", Minusp);
        context.Methods.Add("plusp", Plusp);

        context.Methods.Add("if", If);
        context.Methods.Add("cond", Cond);

        context.Methods.Add("is-nil", (object? s) => s is null);
        context.Methods.Add("is-not-nil", (object? s) => s is not null);

        context.Methods.Add("is-string", (object? s) => s is string);
        context.Methods.Add("is-number", (object? s) =>
            s is
                double or float or decimal
              or
                byte or short or int or long
              or
                sbyte or ushort or uint or ulong
        );
        context.Methods.Add("is-real", (object? s) => s is double or float or decimal);
        context.Methods.Add("is-bool", (object? s) => s is bool);
    }

    bool Gt(dynamic a, dynamic b)
    {
        return a > b;
    }

    bool Gte(dynamic a, dynamic b)
    {
        return a >= b;
    }

    bool Lt(dynamic a, dynamic b)
    {
        return a < b;
    }

    bool Lte(dynamic a, dynamic b)
    {
        return a <= b;
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

    bool Evenp(dynamic a)
    {
        return a % 2 == 0;
    }

    bool Oddp(dynamic a)
    {
        return a % 2 == 0;
    }

    bool Minusp(dynamic a)
    {
        return a < 0;
    }

    bool Plusp(dynamic a)
    {
        return a > 0;
    }

    object? Cond(Atom self)
    {
        foreach (Atom exp in self.GetReader())
        {
            bool cond = exp.GetAtom(0).Nullable()?.GetBoolean() == true;
            if (cond)
            {
                return exp.GetAtom(1).Nullable()?.GetObject();
            }
        }
        return null;
    }

    object? If(Atom self)
    {
        self.EnsureExactItemCount(3, 4);
        bool condition = self.GetAtom(1).GetBoolean();

        if (self.HasKeyword("not"))
            condition = !condition;

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
