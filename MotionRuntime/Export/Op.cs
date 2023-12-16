using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class Op
{
    [RuntimeMethod("eq")]
    public static EvaluationResult Equals(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(2);

        object? first = expression.GetValue(0);
        object? second = expression.GetValue(1);

        if (first != null)
        {
            return new EvaluationResult(first.Equals(second));
        }
        else if (second != null)
        {
            return new EvaluationResult(second.Equals(first));
        }
        else
        {
            // both is null
            return new EvaluationResult(true);
        }
    }

    [RuntimeMethod("or")]
    public static EvaluationResult Or(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(2);

        bool state = false;
        for (int i = 0; i < expression.ArgumentCount; i++)
        {
            state |= (bool)expression.GetValue(i)!;
        }

        return new EvaluationResult(state);
    }

    [RuntimeMethod("and")]
    public static EvaluationResult And(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(2);

        bool state = true;
        for (int i = 0; i < expression.ArgumentCount; i++)
        {
            state &= (bool)expression.GetValue(i)!;
        }

        return new EvaluationResult(state);
    }

    [RuntimeMethod("not")]
    public static EvaluationResult Not(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);

        bool state = expression.GetValue<bool>(0);

        return new EvaluationResult(!state);
    }

    [RuntimeMethod("lt")]
    public static EvaluationResult LessThan(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        double val1 = expression.GetValue<double>(0);
        double val2 = expression.GetValue<double>(1);

        return new EvaluationResult(val1 < val2);
    }

    [RuntimeMethod("elt")]
    public static EvaluationResult LessOrEqualsThan(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        double val1 = expression.GetValue<double>(0);
        double val2 = expression.GetValue<double>(1);

        return new EvaluationResult(val1 <= val2);
    }

    [RuntimeMethod("gt")]
    public static EvaluationResult GreaterThan(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        double val1 = expression.GetValue<double>(0);
        double val2 = expression.GetValue<double>(1);

        return new EvaluationResult(val1 > val2);
    }

    [RuntimeMethod("egt")]
    public static EvaluationResult GreaterOrEqualsThan(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        double val1 = expression.GetValue<double>(0);
        double val2 = expression.GetValue<double>(1);

        return new EvaluationResult(val1 >= val2);
    }
}
