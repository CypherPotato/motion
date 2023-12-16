using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class Console
{
    [RuntimeMethod("write-line")]
    public static EvaluationResult WriteLine(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        object? result = expression.GetValue(0);
        System.Console.WriteLine(result);
        return EvaluationResult.Void;
    }

    [RuntimeMethod("write")]
    public static EvaluationResult Write(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        object? result = expression.GetValue(0);
        System.Console.Write(result);
        return EvaluationResult.Void;
    }

    [RuntimeMethod("console:read-line")]
    public static EvaluationResult ReadLine(InvocationContext expression)
    {
        expression.EnsureArgumentCount(0);
        return new EvaluationResult(System.Console.ReadLine());
    }

    [RuntimeMethod("console:read-key")]
    public static EvaluationResult Read(InvocationContext expression)
    {
        expression.EnsureArgumentCount(0);
        return new EvaluationResult(System.Console.ReadKey().Key);
    }
}
