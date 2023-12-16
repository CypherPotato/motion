using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class Arrays
{
    [RuntimeMethod("make-array")]
    public static EvaluationResult MakeArray(InvocationContext expression)
    {
        ArrayList list = new ArrayList();

        foreach (object? item in expression.GetParamArray())
        {
            list.Add(item);
        }

        return new EvaluationResult(list);
    }

    [RuntimeMethod("aref")]
    public static EvaluationResult Aref(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);

        IList item = expression.GetValue<IList>(0)!;
        double index = expression.GetValue<double>(1);

        return new EvaluationResult(item[(int)index]);
    }

    [RuntimeMethod("array:count")]
    public static EvaluationResult Count(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        ICollection item = expression.GetValue<ICollection>(0)!;       
        return new EvaluationResult((double)item.Count);
    }

    [RuntimeMethod("array:filter-null")]
    public static EvaluationResult FilterNull(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        IList items = expression.GetValue<IList>(0);
        ArrayList result = new ArrayList();

        foreach(var item in items)
        {
            if (item != null)
            {
                result.Add(item);
            }
        }

        return new EvaluationResult(result);
    }
}