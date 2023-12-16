using MotionLang.Interpreter;
using MotionLang.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime.Export;

internal static class String
{
    [RuntimeMethod("str:to-upper")]
    public static EvaluationResult ToUpper(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        string key = expression.GetValue<string>(0);
        return new EvaluationResult(key.ToUpper());
    }

    [RuntimeMethod("str:to-lower")]
    public static EvaluationResult ToLower(InvocationContext expression)
    {
        expression.EnsureArgumentCount(1);
        string key = expression.GetValue<string>(0);
        return new EvaluationResult(key.ToLower());
    }

    [RuntimeMethod("str:split")]
    public static EvaluationResult Split(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string c = expression.GetValue<string>(0);
        string item = expression.GetValue<string>(1);
        return new EvaluationResult(item.Split(c));
    }

    [RuntimeMethod("str:starts-with")]
    public static EvaluationResult StartsWith(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string c = expression.GetValue<string>(0);
        string item = expression.GetValue<string>(1);
        return new EvaluationResult(c.StartsWith(item));
    }

    [RuntimeMethod("str:ends-with")]
    public static EvaluationResult EndsWith(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string c = expression.GetValue<string>(0);
        string item = expression.GetValue<string>(1);
        return new EvaluationResult(c.EndsWith(item));
    }

    [RuntimeMethod("str:contains")]
    public static EvaluationResult Contains(InvocationContext expression)
    {
        expression.EnsureArgumentCount(2);
        string c = expression.GetValue<string>(0);
        string item = expression.GetValue<string>(1);
        return new EvaluationResult(c.Contains(item));
    }

    [RuntimeMethod("str:join")]
    public static EvaluationResult Join(InvocationContext expression)
    {
        string result;
        string c = expression.GetValue<string>(0);
        var items = expression.GetParamArray(1).ToArray();

        if (items.Count() == 1)
        {
            if (items[0] is ArrayList arl)
            {
                result = string.Join(c, arl.ToArray());
            }
            else
            {
                result = string.Join(c, items[0]);
            }
        }
        else
        {
            result = string.Join(c, items);
        }

        return new EvaluationResult(result);
    }

    [RuntimeMethod("str:concat")]
    public static EvaluationResult Concat(InvocationContext expression)
    {
        string result;
        var items = expression.GetParamArray(0).ToArray();

        if (items.Count() == 1)
        {
            if (items[0] is ArrayList arl)
            {
                result = string.Join("", arl.ToArray());
            }
            else
            {
                result = string.Join("", items[0]);
            }
        }
        else
        {
            result = string.Join("", items);
        }

        return new EvaluationResult(result);
    }

    [RuntimeMethod("str:length")]
    public static EvaluationResult Length(InvocationContext expression)
    {
        var item = expression.GetValue<string>(0);
        return new EvaluationResult((double)item.Length);
    }

    [RuntimeMethod("str:index-of")]
    public static EvaluationResult IndexOf(InvocationContext expression)
    {
        string str = expression.GetValue<string>(0);
        string poschar = expression.GetValue<string>(1);

        return new EvaluationResult((double)str.IndexOf(poschar));
    }
}
