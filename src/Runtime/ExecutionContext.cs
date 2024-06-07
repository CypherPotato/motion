using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Motion.Compilation;
using Motion.Parser;
using Motion.Runtime.StandardLibrary;

#pragma warning disable IL3050 

namespace Motion.Runtime;

/// <summary>
/// Represents the scope where the current <see cref="ExecutionContext"/> is being executed.
/// </summary>
public enum ExecutionContextScope
{
    /// <summary>
    /// Specifies that the <see cref="ExecutionContext"/> is running in the global context.
    /// </summary>
    Global,

    /// <summary>
    /// Specifies that an function owns the current <see cref="ExecutionContext"/>.
    /// </summary>
    Function
}

/// <summary>
/// Represents the context in which Motion code is executed.
/// </summary>
public class ExecutionContext
{
    internal CompilationResult compilerResult;
    int level = 0;
    internal StringWriter traceWriter = new StringWriter();
    ExecutionContext global;

    internal MotionCollection<object?> CallingParameters { get; private set; }

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
    /// Gets the defined alises for this execution context.
    /// </summary>
    public MotionCollection<string> Aliases { get; private set; }

    /// <summary>
    /// Gets the defined using statements for this execution context.
    /// </summary>
    public IList<string> UsingStatements { get; private set; }

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
    public ExecutionContext Global => global;

    /// <summary>
    /// Gets the scope of this execution context.
    /// </summary>
    public ExecutionContextScope Scope { get; set; }

    internal static ExecutionContext CreateBaseContext(CompilationResult result)
    {
        var ctx = new ExecutionContext(result, ExecutionContextScope.Global, null, null, 0, new StringWriter());
        ctx.Scope = ExecutionContextScope.Global;

        return ctx;
    }

    private ExecutionContext(CompilationResult result, ExecutionContextScope scope, ExecutionContext? parent, ExecutionContext? global, int level, StringWriter _traceLogger)
    {
        Parent = parent;
        Scope = scope;

        this.compilerResult = result;
        this.level = level;
        this.traceWriter = _traceLogger;
        this.global = global ?? this;

        Variables = new MotionCollection<object?>(this, true, true);
        Constants = new MotionCollection<object?>(this, true, false);
        Methods = new MethodCollection(this, true, false);
        UserFunctions = new MotionCollection<MotionUserFunction>(this, true, false);
        Aliases = new MotionCollection<string>(this, true, false);
        CallingParameters = new MotionCollection<object?>(this, true, false);
        UsingStatements = global?.UsingStatements ?? new List<string>();
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
    /// Tries to resolve an variable, constant or parameter from the specified name. This method searches in the current execution context
    /// and parent ones.
    /// </summary>
    /// <param name="symbolName">The symbol name.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if this <see cref="ExecutionContext"/> contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string symbolName, [MaybeNullWhen(true)] out object? value)
    {
        return TryResolveVariable(symbolName, out value);
    }

    /// <summary>
    /// Tries to resolve an variable, constant or parameter from the specified name and returns it's value, if any. This method searches in the current execution context
    /// and parent ones.
    /// </summary>
    /// <param name="symbolName">The symbol name.</param>
    public object? GetValue(string symbolName)
    {
        TryResolveVariable(symbolName, out var value);
        return value;
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
        this.Aliases = executionContext.Aliases;
        this.UsingStatements = executionContext.UsingStatements;
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

    internal bool IsSymbolDefined(string symbol)
    {
        if (TryResolveAlias(symbol, out _)) return true;
        if (TryResolveMethod(symbol, out _)) return true;
        if (TryResolveUserFunction(symbol, out _)) return true;
        // variables can be overriden in different contexts
        // if (TryResolveVariable(symbol, out _)) return true;
        return false;
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
         || container.Constants.TryGetValue(name, out result)
         || container.CallingParameters.TryGetValue(name, out result))
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

        if (container.Methods.TryGetValue(name, out var del))
        {
            result = LibraryHelper.Create(del!);
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

    bool TryResolveAlias(string name, out string? result)
    {
        ExecutionContext? container = this;

    tryAgain:
        if (container == null)
        {
            // did not found an alias for it. try an using.

            string? lastUsingMatched = null;
            int matchedCount = 0;
            for (int i = 0; i < UsingStatements.Count; i++)
            {
                string usingAlias = UsingStatements[i];
                string sym = usingAlias + ':' + name;

                if (TryResolveMethod(sym, out _) || TryResolveUserFunction(sym, out _) || TryResolveVariable(sym, out _))
                {
                    if (TryResolveMethod(name, out _) || TryResolveUserFunction(name, out _) || TryResolveVariable(name, out _))
                    {
                        throw new Exception($"ambiguous match between '{name}' and '{sym}'");
                    }
                    if (matchedCount == 1)
                    {
                        throw new Exception($"ambiguous match between '{lastUsingMatched}' and '{sym}'");
                    }

                    lastUsingMatched = sym;
                    matchedCount++;
                }
            }

            if (lastUsingMatched is not null)
            {
                result = lastUsingMatched;
                return true;
            }

            result = null;
            return false;
        }

        if (container.Aliases.TryGetValue(name, out result))
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
        return new ExecutionContext(compilerResult, scope, this, global, level + 1, traceWriter);
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
    /// Reads and executes additional code in the current execution context, using the compilation
    /// options that resulted in this instance of <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="additionalCode">The additional code to evaluate.</param>
    public object? Run(string additionalCode)
    {
        var tokenizer = new Tokenizer(CompilerSource.FromCode(additionalCode), compilerResult.Options);
        tokenizer.Read();

        var result = tokenizer.Result;
        for (int i = 0; i < result.Length; i++)
        {
            if (i == result.Length - 1)
            {
                return EvaluateTokenItem(result[i], AtomBase.Undefined);
            }
            else
            {
                EvaluateTokenItem(result[i], AtomBase.Undefined);
            }
        }

        return null;
    }

    /// <summary>
    /// Executes the compiled code and returns all evaluated results from defined atoms in this context.
    /// </summary>
    /// <returns>The evaluated objects from the defined atoms.</returns>
    public object? Evaluate()
    {
        try
        {
            for (int i = 0; i < compilerResult.tokens.Length; i++)
            {
                if (CancellationToken?.IsCancellationRequested == true)
                    return null;

                AtomBase t = compilerResult.tokens[i];
                if (i == compilerResult.tokens.Length - 1)
                {
                    return EvaluateTokenItem(t, AtomBase.Undefined);
                }
                else
                {
                    EvaluateTokenItem(t, AtomBase.Undefined);
                }
            }
            return null;
        }
        catch (MotionExitException e)
        {
            return e.Result;
        }
    }

    /// <summary>
    /// Executes the compiled code asynchronously and returns the evaluated result from defined atoms in this context.
    /// </summary>
    /// <returns>The evaluated objects from the defined atoms.</returns>
    public Task<object?> EvaluateAsync()
    {
        return Task.Run(Evaluate);
    }

    internal object? EvaluateTokenItem(AtomBase t, AtomBase parent)
    {
        object? result = null;
        Exception? exception = null;
        TextInterpreterSnapshot expSymbolLocation = default;

        if (CancellationToken?.IsCancellationRequested == true)
        {
            throw new MotionExitException(null);
        }
        try
        {
            switch (t.Type)
            {
                case TokenType.String:
                case TokenType.Character:
                case TokenType.Number:
                case TokenType.Boolean:
                    // literal value stored into the atom contents
                    return t.Content;

                case TokenType.Null:
                case TokenType.Undefined:
                    return null;

                case TokenType.Operator:
                case TokenType.Keyword:
                    throw new MotionException("this atom cannot be evaluated to an value without an expression.", t.Location, null);

                case TokenType.ClrType:
                    if (!compilerResult.Options.ExposeCLR)
                    {
                        throw new MotionException("CLR access is not allowed in this context.", t.Location, null);
                    }

                    string expression = t.Content!.ToString()!;
                    string[] parts = expression.Split('/');
                    Type? rType;

                    if (parts.Length > 1)
                    {
                        rType = TypeHelper.ResolveType(parts[0] + '`' + (parts.Length - 1));
                    }
                    else
                    {
                        rType = TypeHelper.ResolveType(parts[0]);
                    }

                    if (rType is null)
                    {
                        throw new MotionException($"unknown or undefined type '{parts[0]}'.", t.Location, null);
                    }

                    if (parts.Length > 1)
                    {
                        Type[] genericTypes = new Type[parts.Length - 1];
                        for (int i = 0; i < genericTypes.Length; i++)
                        {
                            Type? gType = TypeHelper.ResolveType(parts[i + 1]);

                            if (gType is null)
                            {
                                throw new MotionException($"unknown or undefined generic type '{parts[i]}' at position {i + 1}.", t.Location, null);
                            }
                            else
                            {
                                genericTypes[i] = gType;
                            }
                        }

                        rType = rType.MakeGenericType(genericTypes);
                    }

                    return rType;

                case TokenType.Symbol:
                    {
                        string name = t.Content!.ToString()!;

                        if (TryResolveAlias(name, out string? aliasedName))
                        {
                            if (aliasedName is null)
                                throw new MotionException($"the alias name \"{name}\" points to an null reference alias.", t.Location, null);

                            name = aliasedName;
                        }
                        ;

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

                case TokenType.Array:
                    {
                        object?[] items = new object?[t.Children.Length];

                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = EvaluateTokenItem(t.Children[i], t);
                        }

                        return items;
                    }

                case TokenType.Expression:
                    {
                        int count = t.Children.Length;
                        if (count == 0)
                        {
                            return null;
                        }
                        else if (count == 1)
                        {
                            return EvaluateTokenItem(t.Children[0], t);
                        }
                        else
                        {
                            var firstChildren = t.Children[0];
                            if (firstChildren.Type is not TokenType.Symbol and not TokenType.ClrSymbol and not TokenType.Operator)
                            {
                                // run the last value
                                for (int i = 0; i < count; i++)
                                {
                                    if (i == count - 1)
                                    {
                                        return EvaluateTokenItem(t.Children[i], t);
                                    }
                                    else
                                    {
                                        EvaluateTokenItem(t.Children[i], t);
                                    }
                                }
                            }
                            else if (firstChildren.Type == TokenType.ClrSymbol)
                            {
                                if (!compilerResult.Options.ExposeCLR)
                                {
                                    throw new MotionException("CLR access is not allowed in this context.", t.Location, null);
                                }
                                if (t.Children.Length == 1)
                                {
                                    throw new MotionException("object expected.", t.Location, null);
                                }

                                string symbol = firstChildren.Content!.ToString()!;
                                object? value = EvaluateTokenItem(t.Children[1], t);

                                if (value is null)
                                {
                                    throw new MotionException("null reference.", t.Children[1].Location, null);
                                }

                                List<Type?> typeHints = new List<Type?>(t.Children.Length);

                                for (int i = 2; i < t.Children.Length; i++)
                                {
                                    var child = t.Children[i];
                                    Type? childType = TypeHelper.ResolveTypeByAtom(ref child);
                                    typeHints.Add(childType);
                                }

                                Type objType = value.GetType();

                                BindingFlags bflag =
                                      BindingFlags.Public
                                    | BindingFlags.Instance
                                    | BindingFlags.IgnoreCase
                                    ;

                                MethodInfo[] methods = objType.GetMethods(bflag);
                                MethodInfo? publicMethod = TypeHelper.FindMatchingMethodInfo(symbol, methods, typeHints, t.Children.Length == 2);

                                if (publicMethod is null)
                                {
                                    if (typeHints.Count == 0)
                                    {
                                        throw new MotionException($"'{objType.FullName}' does not contain a public method for '{symbol}'.", t.Children[1].Location, null);
                                    }
                                    else
                                    {
                                        throw new MotionException($"'{objType.FullName}' does not contain a public method for '{symbol}' that accepts the parameters:\n- {string.Join("- ", typeHints.Select(s => s.FullName + "\n"))}", t.Children[1].Location, null);
                                    }
                                }

                                return LibraryHelper.InvokeMethodInfo(publicMethod, new Atom(t, parent, this), value);
                            }

                            expSymbolLocation = firstChildren.Location;
                            string name = firstChildren.Content!.ToString()!;

                            if (TryResolveAlias(name, out string? aliasedName))
                            {
                                if (aliasedName is null)
                                    throw new MotionException($"the alias name \"{name}\" points to an null reference alias.", t.Location, null);

                                name = aliasedName;
                            }
                            ;

                            if (TryResolveMethod(name, out var methodValue))
                            {
                                result = methodValue!.Invoke(new Atom(t, parent, this));
                                return result;
                            }
                            else
                            {
                                if (TryResolveUserFunction(name, out var userFuncValue))
                                {
                                    result = userFuncValue!.Invoke(t, Fork(ExecutionContextScope.Function));
                                    return result;
                                }
                                else
                                {
                                    string nameL = name.ToLower();
                                    string[] similar =
                                        Methods.Keys.Concat(UserFunctions.Keys)
                                        .Select(f => f.ToLower())
                                        .Where(f => f.Contains(nameL) || StdString.ComputeLevenshteinDistance(f, nameL) <= 2)
                                        .Take(5)
                                        .ToArray();

                                    string errMessage = "method not defined: " + name;

                                    if (similar.Length > 0)
                                    {
                                        errMessage += "\n\ndid you mean one of these methods?\n- " + string.Join("\n- ", similar);
                                    }

                                    throw new MotionException(errMessage, firstChildren.Location, null);
                                }
                            }
                        }
                    }

                default:
                    return null;
            }
        }
        catch (MotionExitException mexi)
        {
            exception = mexi;
            throw;
        }
        catch (MotionException me)
        {
            exception = me;
            throw;
        }
        catch (TargetInvocationException tex)
        {
            if (tex.InnerException is MotionException mex)
            {
                exception = mex;
                throw mex;
            }

            TextInterpreterSnapshot location;
            if (expSymbolLocation.Initialized)
            {
                location = expSymbolLocation;
            }
            else
            {
                location = t.Location;
            }

            exception = tex;
            throw new MotionException($"exception caught in Atom {t}:\n{tex.InnerException?.Message}", location, tex);
        }
        catch (Exception ex)
        {
            TextInterpreterSnapshot location;
            if (expSymbolLocation.Initialized)
            {
                location = expSymbolLocation;
            }
            else
            {
                location = t.Location;
            }

            exception = ex;
            throw new MotionException($"exception caught in Atom {t}:\n{ex.Message}", location, ex);
        }
        finally
        {
            ;
        }
    }
}
