using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class Common
{
    [RuntimeMethod("if")]
    public static EvaluationResult If(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(2);

        bool cond = expression.GetValue<bool>(0);
        object? state;

        if (cond)
        {
            state = expression.GetValue(1);
        }
        else if (expression.ArgumentCount > 2)
        {
            state = expression.GetValue(2);
        }
        else
        {
            state = null;
        }

        return new EvaluationResult(state);
    }

    [RuntimeMethod("map")]
    public static EvaluationResult Map(InvocationContext expression)
    {
        expression.EnsureArgumentCount(3);

        var mapContext = expression.Context.Fork(ContextScope.Function);

        ArrayList result = new ArrayList();
        IList items = expression.GetValue<IList>(0)!;
        string symbol = expression.GetSymbol(1);
        Expression block = expression.GetExpression(2);

        foreach (var item in items)
        {
            expression.Context.Variables[symbol] = item;
            var resultItem = expression.EvaluateExpression(block, mapContext);
            result.Add(resultItem.Result);
        }

        return new EvaluationResult(result);
    }

    [RuntimeMethod("while")]
    public static EvaluationResult While(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        while (true)
        {
            bool cond = expression.GetValue<bool>(0);
            if (!cond)
            {
                break;
            }
            else
            {
                expression.GetValue(1);
            }
        }

        return EvaluationResult.Void;
    }
}
