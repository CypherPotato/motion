using MotionLang.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Runtime.Common;

internal class StandardRuntime
{
    public static EvaluationResult DefineFunction(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(3);

        string name = expression.GetSymbol(0);
        Expression argsExpression = expression.GetExpression(1);
        Expression body;

        if (expression.GetTokenType(2) == MotionLang.Compiler.TokenType.Expression)
        {
            body = expression.GetExpression(2);
        }
        else
        {
            // optional doc at 2
            body = expression.GetExpression(3);
        }

        string[] args = argsExpression.GetSymbols().ToArray();
        var assembly = expression.Context.compilationResult;
        assembly.UserFunctions.Add(name, new UserFunction(args, body));

        return EvaluationResult.Void;
    }

    public static EvaluationResult Const(InvocationContext expression)
    {
        expression.EnsureMinimumArgumentCount(2);

        string name = expression.GetSymbol(0);
        var valueType = expression.GetTokenType(1);

        expression.EnsureTokenType(1, new Compiler.TokenType[] {
            Compiler.TokenType.String,
            Compiler.TokenType.Number,
            Compiler.TokenType.Symbol,
            Compiler.TokenType.Boolean,
            Compiler.TokenType.Null
        });

        object? value = expression.GetValue(1);

        var assembly = expression.Context.compilationResult;
        assembly.Constants.Add(name, value);

        return EvaluationResult.Void;
    }
}
