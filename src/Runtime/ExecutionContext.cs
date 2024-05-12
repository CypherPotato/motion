using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Runtime.StandardLibrary;

namespace Motion.Runtime;

public enum ExecutionContextScope
{
    Global,
    Function
}

/// <summary>
/// Represents the context in which Motion code is executed.
/// </summary>
public class ExecutionContext
{
    int scope = Random.Shared.Next();
    private AtomBase[] baseTokens;

    /// <summary>
    /// Gets the stored variables defined within this execution context.
    /// </summary>
    public MotionCollection<object?> Variables { get; private set; }

    /// <summary>
    /// Gets the stored constants defined within this execution context.
    /// </summary>
    public MotionCollection<object?> Constants { get; private set; }

    /// <summary>
    /// Gets the defined library-class methods within this execution context.
    /// </summary>
    public MethodCollection Methods { get; private set; }

    /// <summary>
    /// Gets the user defined methods within this execution context.
    /// </summary>
    public MotionCollection<MotionUserFunction> UserFunctions { get; private set; }

    /// <summary>
    /// Gets the parent execution context, if any.
    /// </summary>
    public ExecutionContext? Parent { get; }

    /// <summary>
    /// Gets or sets the atom invocation cancellation token of this context.
    /// </summary>
    public CancellationToken? CancellationToken { get; set; }

    /// <summary>
    /// Gets the global execution context.
    /// </summary>
    public ExecutionContext Global
    {
        get
        {
            ExecutionContext find = this;

            while (find.Parent != null)
            {
                find = find.Parent;
            }

            return find;
        }
    }

    /// <summary>
    /// Gets the scope of this execution context.
    /// </summary>
    public ExecutionContextScope Scope { get; set; }

    internal static ExecutionContext CreateBaseContext(AtomBase[] tokens)
    {
        var ctx = new ExecutionContext(tokens, null);
        ctx.Scope = ExecutionContextScope.Global;

        return ctx;
    }

    private ExecutionContext(AtomBase[] tokens, ExecutionContext? parent)
    {
        Parent = parent;
        baseTokens = tokens;
        Variables = new MotionCollection<object?>(this, true, true);
        Constants = new MotionCollection<object?>(this, true, false);
        Methods = new MethodCollection(this, true, false);
        UserFunctions = new MotionCollection<MotionUserFunction>(this, true, false);
    }

    internal void ImportLibrary(IMotionLibrary library)
    {
        Methods.StartNamespace(library.Namespace);
        Constants.StartNamespace(library.Namespace);

        library.ApplyMembers(this);

        Methods.EndNamespace();
        Constants.EndNamespace();
    }

    /// <summary>
    /// Imports methods, variables and user functions from another <see cref="ExecutionContext"/>, replacing the actual
    /// instance with the ones from the another context.
    /// </summary>
    /// <param name="executionContext">The <see cref="ExecutionContext"/> to import data from.</param>
    public void ImportState(ExecutionContext? executionContext)
    {
        if (executionContext == null) return;
        this.Methods = executionContext.Methods;
        this.Variables = executionContext.Variables;
        this.UserFunctions = executionContext.UserFunctions;
        this.Constants = executionContext.Constants;
    }

    /// <summary>
    /// Gets the parent <see cref="ExecutionContext"/> that is of the specified scope.
    /// </summary>
    /// <param name="scope">The scope of the parent <see cref="ExecutionContext"/>.</param>
    public ExecutionContext GetScope(ExecutionContextScope scope)
    {
        ExecutionContext find = this;

        while (find.Scope != scope && find.Parent != null)
        {
            find = find.Parent;
        }

        return find;
    }

    bool TryResolveVariable(string name, out object? result)
    {
        ExecutionContext? container = this;

    tryAgain:

        if (container == null)
        {
            result = null;
            return false;
        }

        if (container.Variables.TryGetValue(name, out result)
         || container.Constants.TryGetValue(name, out result))
        {
            return true;
        }
        else
        {
            container = container.Parent;
            goto tryAgain;
        }
    }

    bool TryResolveMethod(string name, out MotionMethod? result)
    {
        ExecutionContext? container = this;

    tryAgain:

        if (container == null)
        {
            result = null;
            return false;
        }

        if (container.Methods.TryGetValue(name, out result))
        {
            return true;
        }
        else
        {
            container = container.Parent;
            goto tryAgain;
        }
    }

    bool TryResolveUserFunction(string name, out MotionUserFunction? result)
    {
        ExecutionContext? container = this;

    tryAgain:

        if (container == null)
        {
            result = null;
            return false;
        }

        if (container.UserFunctions.TryGetValue(name, out result))
        {
            return true;
        }
        else
        {
            container = container.Parent;
            goto tryAgain;
        }
    }

    ExecutionContext Fork(ExecutionContextScope scope)
    {
        return new ExecutionContext(baseTokens, this) { Scope = scope };
    }

    /// <summary>
    /// Sets an variable in this <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="key">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>Return this self <see cref="ExecutionContext"/> instance.</returns>
    public ExecutionContext SetVariable(string key, object? value)
    {
        Variables.Set(key, value);
        return this;
    }

    /// <summary>
    /// Sets an constant in this <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="key">The constant name.</param>
    /// <param name="value">The constant value.</param>
    /// <returns>Return this self <see cref="ExecutionContext"/> instance.</returns>
    public ExecutionContext SetConstant(string key, object? value)
    {
        Constants.Set(key, value);
        return this;
    }

    /// <summary>
    /// Executes the compiled code and returns all evaluated results from defined atoms in this context.
    /// </summary>
    /// <returns>The evaluated objects from the defined atoms.</returns>
    public object?[] Evaluate()
    {
        Task<object?[]> evalTask = EvaluateAsync();
        return evalTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes the compiled code asynchronously and returns the evaluated result from defined atoms in this context.
    /// </summary>
    /// <returns>The evaluated objects from the defined atoms.</returns>
    public async Task<object?[]> EvaluateAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                object?[] results = new object[baseTokens.Length];

                for (int i = 0; i < baseTokens.Length; i++)
                {
                    if (CancellationToken?.IsCancellationRequested == true)
                        return results;

                    AtomBase t = baseTokens[i];
                    results[i] = EvaluateTokenItem(t, AtomBase.Undefined);
                }

                return results;
            }
            catch (MotionExitException ext)
            {
                return new object?[] { ext.Result };
            }
        });
    }

    internal object? EvaluateTokenItem(AtomBase t, AtomBase parent)
    {
        if (CancellationToken?.IsCancellationRequested == true)
        {
            throw new MotionExitException(null);
        }
        try
        {
            switch (t.Type)
            {
                case TokenType.String:
                    return (string)t.Content!;
                case TokenType.Number:
                    return t.Content!; // can be int or double
                case TokenType.Boolean:
                    return (bool)t.Content!;

                case TokenType.Null:
                case TokenType.Undefined:
                    return null;

                case TokenType.Operator:
                case TokenType.Keyword:
                    throw new MotionException("this atom cannot be evaluated to an value without an expression.", t.Location, null);

                case TokenType.Symbol:
                    {
                        string name = t.Content!.ToString()!;

                        if (TryResolveMethod(name, out var methodValue))
                        {
                            // invoke in the parent token which probably holds the expression
                            return methodValue!.Invoke(new Atom(parent, AtomBase.Undefined, this));
                        }
                        else if (TryResolveUserFunction(name, out var userFuncValue))
                        {
                            return userFuncValue!.Invoke(t, Fork(ExecutionContextScope.Function));
                        }
                        else if (TryResolveVariable(name, out var varValue))
                        {
                            return varValue;
                        }
                        else
                        {
                            throw new MotionException("unresolved variable, constant or method: " + name, t.Location, null);
                        }
                    }

                case TokenType.Expression:
                    {
                        if (t.Children.Length == 0)
                        {
                            return null;
                        }
                        else if (t.Children.Length == 1)
                        {
                            return EvaluateTokenItem(t.Children[0], t);
                        }
                        else
                        {
                            var firstChildren = t.Children[0];
                            if (firstChildren.Type != TokenType.Symbol && firstChildren.Type != TokenType.Operator)
                            {
                                int len = t.Children.Length;
                                for (int i = 0; i < len; i++)
                                {
                                    AtomBase T = t.Children[i];
                                    if (i == len - 1)
                                    {
                                        return EvaluateTokenItem(T, t);
                                    }
                                    else EvaluateTokenItem(T, t);
                                }
                            }

                            string name = firstChildren.Content!.ToString()!;
                            if (TryResolveMethod(name, out var methodValue))
                            {
                                return methodValue!.Invoke(new Atom(t, parent, this));
                            }
                            else
                            {
                                if (TryResolveUserFunction(name, out var userFuncValue))
                                {
                                    return userFuncValue!.Invoke(t, Fork(ExecutionContextScope.Function));
                                }
                                else
                                {
                                    throw new MotionException("method not defined: " + name, firstChildren.Location, null);
                                }
                            }
                        }
                    }

                default:
                    return null;
            }
        }
        catch (MotionExitException)
        {
            throw;
        }
        catch (MotionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MotionException($"Exception caught in Atom {t}: {ex.Message}", t.Location, ex);
        }
    }
}
