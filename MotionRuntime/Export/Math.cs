using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class Math
{
    [RuntimeMethod("math:sum")]
    public static EvaluationResult Sum(InvocationContext expression)
    {
        double carry = 0;

        for (int i = 0; i < expression.ArgumentCount; i++)
        {
            carry += expression.GetValue<double>(i);
        }

        return new EvaluationResult(carry);
    }

    [RuntimeMethod("math:mult")]
    public static EvaluationResult Multiply(InvocationContext expression)
    {
        double carry = expression.GetValue<double>(0);

        for (int i = 1; i < expression.ArgumentCount; i++)
        {
            carry *= expression.GetValue<double>(i);
        }

        return new EvaluationResult(carry);
    }

    [RuntimeMethod("math:div")]
    public static EvaluationResult Divide(InvocationContext expression)
    {
        double carry = expression.GetValue<double>(0);

        for (int i = 1; i < expression.ArgumentCount; i++)
        {
            carry /= expression.GetValue<double>(i);
        }

        return new EvaluationResult(carry);
    }
}
