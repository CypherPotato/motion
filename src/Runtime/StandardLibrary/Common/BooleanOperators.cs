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

        context.Methods.Add("<", (dynamic A, dynamic B) => A < B);
        context.Methods.Add("<=", (dynamic A, dynamic B) => A <= B);
        context.Methods.Add(">", (dynamic A, dynamic B) => A > B);
        context.Methods.Add(">=", (dynamic A, dynamic B) => A >= B);

        context.Methods.Add("zerop", Zerop);
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
}
