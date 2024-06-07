using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdCType : IMotionLibrary
{
    public string? Namespace => "ctype";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("to-string", (object? n) => Convert.ToString(n));

        context.Methods.Add("to-int16", (object? n) => Convert.ToInt16(n));
        context.Methods.Add("to-int32", (object? n) => Convert.ToInt32(n));
        context.Methods.Add("to-int64", (object? n) => Convert.ToInt64(n));

        context.Methods.Add("to-uint16", (object? n) => Convert.ToUInt16(n));
        context.Methods.Add("to-uint32", (object? n) => Convert.ToUInt32(n));
        context.Methods.Add("to-uint64", (object? n) => Convert.ToUInt64(n));

        context.Methods.Add("to-single", (object? n) => Convert.ToSingle(n));
        context.Methods.Add("to-double", (object? n) => Convert.ToDouble(n));
        context.Methods.Add("to-decimal", (object? n) => Convert.ToDecimal(n));

        context.Methods.Add("to-byte", (object? n) => Convert.ToByte(n));
        context.Methods.Add("to-sbyte", (object? n) => Convert.ToSByte(n));

        context.Methods.Add("to-bool", (object? n) => Convert.ToBoolean(n));
    }
}
