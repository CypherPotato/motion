using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class BitwiseOperators : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("logand", (dynamic a, dynamic b) => a & b);
        context.Methods.Add("logior", (dynamic a, dynamic b) => a | b);
        context.Methods.Add("logxor", (dynamic a, dynamic b) => a ^ b);
        context.Methods.Add("lognor", (dynamic a, dynamic b) => a & ~b);
        context.Methods.Add("logeqv", (dynamic a, dynamic b) => a ^ ~b);
        context.Methods.Add("logcount", (dynamic a) => a & ~(a >> 1));

        context.Methods.Add("eflag", (Enum a, Enum b) => a.HasFlag(b));
        context.Methods.Add("econstruct", EConstruct);
    }

    object EConstruct(params object[] values)
    {
        if (values.Length == 0)
        {
            if (values.Length == 0) throw new ArgumentException("At least one operand is required.");
        }
        else if (values.Length == 1)
        {
            return values[0];
        }

        long n = Convert.ToInt64(values[0]);
        for(int i = 1; i < values.Length; i++)
        {
            n |= Convert.ToInt64(values[1]);
        }

        return Enum.ToObject(values[0].GetType(), n);
    }
}
