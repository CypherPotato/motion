using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdMath : IMotionLibrary
{
    public string? Namespace => "math";

    public void ApplyMembers(Motion.Runtime.ExecutionContext context)
    {
        context.Constants.Set("pi", System.Math.PI);
        context.Constants.Set("e", System.Math.E);
        context.Methods.Set("pow", atom =>
        {
            atom.EnsureExactItemCount(3);

            double num = atom.GetAtom(1).GetDouble();
            double exp = atom.GetAtom(2).GetDouble();

            return System.Math.Pow(num, exp);
        });
        context.Methods.Set("truncate", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return Math.Truncate((double)2);
        });
        context.Methods.Set("abs", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return Math.Abs((double)2);
        });
        context.Methods.Set("intl", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return (int)d;
        });
        context.Methods.Set("ceil", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return Math.Ceiling((double)d);
        });
        context.Methods.Set("floor", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return Math.Floor((double)d);
        });
        context.Methods.Set("round", atom =>
        {
            atom.EnsureExactItemCount(2);

            double d = atom.GetAtom(1).GetDouble();
            return Math.Round((double)d);
        });
    }
}
