using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdMath : IMotionLibrary
{
    public string? Namespace => "math";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("pow", (double x, double y) => Math.Pow(x, y));

        context.Methods.Add("log", (double x) => Math.Log(x));
        context.Methods.Add("log10", (double x) => Math.Log10(x));
        context.Methods.Add("log2", (double x) => Math.Log2(x));
        context.Methods.Add("ilogb", (double x) => Math.ILogB(x));

        context.Methods.Add("abs", (double x) => Math.Abs(x));
        context.Methods.Add("trunc", (double x) => Math.Truncate(x));

        context.Methods.Add("min", (double x, double y) => Math.Min(x, y));
        context.Methods.Add("max", (double x, double y) => Math.Max(x, y));

        context.Methods.Add("ceil", (double x) => Math.Ceiling(x));
        context.Methods.Add("floor", (double x) => Math.Floor(x));
        context.Methods.Add("round", (double x) => Math.Round(x));
        context.Methods.Add("roundn", (double x, int nums) => Math.Round(x, nums));

        context.Methods.Add("cbrt", (double x) => Math.Cbrt(x));
        context.Methods.Add("sqrt", (double x) => Math.Sqrt(x));

        context.Methods.Add("sign", (decimal x) => Math.Sign(x));

        context.Methods.Add("tan", (double x) => Math.Tan(x));
        context.Methods.Add("tanh", (double x) => Math.Tanh(x));
        context.Methods.Add("atan", (double x) => Math.Atan(x));
        context.Methods.Add("atanh", (double x) => Math.Atanh(x));

        context.Methods.Add("cos", (double x) => Math.Cos(x));
        context.Methods.Add("cosh", (double x) => Math.Cosh(x));
        context.Methods.Add("acos", (double x) => Math.Acos(x));
        context.Methods.Add("acosh", (double x) => Math.Acosh(x));

        context.Methods.Add("sin", (double x) => Math.Sin(x));
        context.Methods.Add("sinh", (double x) => Math.Sinh(x));
        context.Methods.Add("asin", (double x) => Math.Asin(x));
        context.Methods.Add("asinh", (double x) => Math.Asinh(x));
    }
}
