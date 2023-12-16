using MotionLang.Compiler;
using MotionLang.Provider;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MotionLang.Interpreter;

public class Expression
{
    internal Token expToken;

    internal Expression(Token expToken)
    {
        this.expToken = expToken;
    }

    public IEnumerable<string> GetSymbols()
    {
        int pos = 0;
        foreach (Token t in expToken.Children)
        {
            pos++;
            if (t.Type != TokenType.Symbol)
            {
                throw new MotionException($"expression item {pos}: cannot convert from {t.Type} to Symbol.", t.Location, null);
            }
            else
            {
                yield return t.Content!.ToString()!;
            }
        }
    }

    internal static EvaluationResult EvaluateToken(RuntimeContext currentScope, ref Token token)
    {
        if (token.Flags.HasFlag(TokenFlag.CompileTime))
        {
            return EvaluationResult.Void;
        }
        else if (token.Type == TokenType.Number)
        {
            return new EvaluationResult()
            {
                IsVoid = false,
                Result = token.Content
            };
        }
        else if (token.Type == TokenType.String)
        {
            return new EvaluationResult()
            {
                IsVoid = false,
                Result = token.Content!.ToString()
            };
        }
        else if (token.Type == TokenType.Boolean)
        {
            return new EvaluationResult()
            {
                IsVoid = false,
                Result = token.Content
            };
        }
        else if (token.Type == TokenType.Null)
        {
            return new EvaluationResult()
            {
                IsVoid = false,
                Result = null
            };
        }
        else if (token.Type == TokenType.Symbol)
        {
            string name = token.Content!.ToString()!;

            if (currentScope.compilationResult.Constants.TryGetValue(name, out var constantValue))
            {
                return new EvaluationResult()
                {
                    IsVoid = false,
                    Result = constantValue
                };
            }
            else if (currentScope.TryGetUnderlyingVariable(name, out var result))
            {
                return new EvaluationResult()
                {
                    IsVoid = false,
                    Result = result
                };
            }
            else
            {
                throw new MotionException($"undefined variable or constant: {name}", token.Location, null);
            }
        }
        else //if (token.Type == TokenType.Expression)
        {
            if (token.Children.Length == 0)
            {
                return EvaluationResult.Void;
            }

            if (token.Children[0].Type == TokenType.Symbol)
            {
                string methodName = token.Children[0].Content!.ToString()!;

                CompilationResult assembly = currentScope.compilationResult;

                if (currentScope.isCompileTime)
                {
                    if (assembly.Runtime.compTimeMethods.TryGetValue(methodName, out RuntimeMethod? compileMethod))
                    {
                        token.Flags = TokenFlag.CompileTime;
                        InvocationContext invoke = new InvocationContext(currentScope)
                        {
                            baseExpression = token
                        };
                        try
                        {
                            return compileMethod!.Invoke(invoke);
                        }
                        catch (MotionException mex)
                        {
                            throw new Compiler.MotionException($"an error was thrown from the compiler method \"{methodName}\": {mex.Message}", mex.snapshot, mex);
                        }
                        catch (Exception ex)
                        {
                            throw new Compiler.MotionException($"an error was thrown from the compiler method \"{methodName}\": {ex.Message}", token.Location, ex);
                        }
                    }
                    else
                    {
                        return EvaluationResult.Void;
                    }
                }

                if (assembly.Runtime.runtimeMethods.TryGetValue(methodName, out RuntimeMethod? runtimeMethod))
                {
                    InvocationContext invoke = new InvocationContext(currentScope)
                    {
                        baseExpression = token
                    };
                    return runtimeMethod!.Invoke(invoke);
                }
                else if (assembly.UserFunctions.TryGetValue(methodName, out UserFunction? userFunction))
                {
                    int argCount = userFunction.Arguments.Length;
                    if (token.Children.Length - 1 != argCount)
                    {
                        throw new MotionException($"the user function {methodName} expects {argCount} arguments. got {token.Children.Length - 1} instead.", token.Children[0].Location, null);
                    }

                    int argIndex = 1;
                    foreach (string s in userFunction.Arguments)
                    {
                        currentScope.Variables[s] = EvaluateToken(currentScope, ref token.Children[argIndex]).Result;
                        argIndex++;
                    }

                    var functionScope = currentScope.Fork(ContextScope.Function);

                    try
                    {
                        EvaluateToken(functionScope, ref userFunction.Body.expToken);
                    }
                    catch (Exception ex)
                    {
                        throw new MotionException($"an error was thrown from the runtime method \"{methodName}\": {ex.Message}", token.Location, ex);
                    }

                    return functionScope.Result;
                }
                else
                {
                    throw new MotionException($"the method or user function \"{methodName}\" is not defined.", token.Children[0].Location, null);
                }
            }
            else
            {
                for (int i = 0; i < token.Children.Length; i++)
                {
                    EvaluateToken(currentScope, ref token.Children[i]);
                }
                return currentScope.Result;
            }
        }
    }
}
