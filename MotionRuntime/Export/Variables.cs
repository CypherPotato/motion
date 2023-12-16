using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;
internal static class Variables
{
    [RuntimeMethod("set")]
    public static EvaluationResult SetVariable(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string key = expression.GetSymbol(0);
        object? value = expression.GetValue(1);

        var scope = expression.Context.GetCurrentScopeContext();
        scope.Variables[key] = value;

        return new EvaluationResult(value);
    }

    [RuntimeMethod("set-global")]
    public static EvaluationResult SetGlobalVariable(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string key = expression.GetSymbol(0);
        object? value = expression.GetValue(1);

        var global = expression.Context.GetGlobalContext();
        global.Variables[key] = value;

        return new EvaluationResult(value);
    }

    [RuntimeMethod("return")]
    public static EvaluationResult Return(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        object? value = expression.GetValue(0);

        var scope = expression.Context;
        scope.Result = new EvaluationResult(value);

        return scope.Result;
    }
}
