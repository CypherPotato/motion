using MotionLang.Compiler;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Interpreter;

public class InvocationContext
{
    internal Token baseExpression;

    public string MethodName { get => baseExpression.Children[0].Content!.ToString()!; }
    public RuntimeContext Context { get; private set; }
    public int ArgumentCount { get => baseExpression.Children.Length - 1; }

    internal InvocationContext(RuntimeContext context)
    {
        Context = context;
    }

    private Token GetToken(int index, TokenType expect)
    {
        var t = baseExpression.Children[index + 1];
        if (t.Type != expect)
        {
            throw new MotionException($"at method {MethodName}, argument {index + 1}: cannot convert from {t.Type} to {expect}.", t.Location, null);
        }
        return t;
    }

    public void EnsureTokenType(int index, TokenType[] allowedTypes)
    {
        var token = baseExpression.Children[index + 1];
        if (!allowedTypes.Contains(token.Type))
        {
            throw new MotionException($"at method {MethodName}, argument {index + 1}, invalid token. expected any of {string.Join(", ", allowedTypes)}", token.Location, null);
        }
    }

    public void EnsureMinimumArgumentCount(int minArgumentCount)
    {
        if (ArgumentCount < minArgumentCount)
        {
            throw new MotionException($"at method {MethodName}, expected at least {minArgumentCount} arguments. got {ArgumentCount}.", baseExpression.Location, null);
        }
    }

    public void EnsureArgumentCount(int argumentCount)
    {
        if (ArgumentCount != argumentCount)
        {
            throw new MotionException($"at method {MethodName}, expected {argumentCount} arguments. got {ArgumentCount}.", baseExpression.Location, null);
        }
    }

    public IEnumerable<object?> GetParamArray(int fromIndex = 0)
    {
        for (int i = fromIndex; i < ArgumentCount; i++)
        {
            yield return GetValue(i);
        }
    }

    public EvaluationResult EvaluateExpression(Expression expression, RuntimeContext scope)
    {
        return Expression.EvaluateToken(scope, ref expression.expToken);
    }

    public Expression GetExpression(int index)
    {
        return new Expression(GetToken(index, TokenType.Expression));
    }

    public TokenType GetTokenType(int index)
    {
        return baseExpression.Children[index + 1].Type;
    }

    public T GetValue<T>(int index)
    {
        return (T)GetValue(index)!;
    }

    public object? GetValue(int index)
    {
        Token t = baseExpression.Children[index + 1];
        return Expression.EvaluateToken(Context, ref t).Result;
    }

    public string GetSymbol(int index)
    {
        return GetToken(index, TokenType.Symbol).Content!.ToString()!;
    }
}
