using MotionLang.Compiler;
using MotionLang.Interpreter;
using MotionLang.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Runtime;

public class RuntimeContext
{
    public Dictionary<string, object?> Variables { get; set; }
    public EvaluationResult Result { get; set; }
    public ContextScope Scope { get; private set; }
    public Guid ContextId { get; private set; }

    internal bool isCompileTime;
    internal CompilationResult compilationResult;
    internal readonly RuntimeContext? parent;

    internal RuntimeContext(CompilationResult compilationResult, RuntimeContext? parent, ContextScope mode)
    {
        this.parent = parent;
        this.compilationResult = compilationResult;
        this.Scope = mode;
        this.Result = EvaluationResult.Void;
        this.Variables = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        this.ContextId = Guid.NewGuid();
    }

    public RuntimeContext Fork(ContextScope mode)
    {
        if (mode == ContextScope.Global) throw new ArgumentException("Cannot fork an global scope.");
        return new RuntimeContext(compilationResult, this, mode);
    }

    public bool TryGetUnderlyingVariable(string name, out object? result)
    {
        bool __tryGetUnderlyingVariable(RuntimeContext context, int level, string name, out object? result)
        {
            if (context.Variables.TryGetValue(name, out result) == true)
            {
                return true;
            }

            if (level < 256 && context.parent != null)
            {
                return __tryGetUnderlyingVariable(context.parent, level + 1, name, out result);
            }
            else
            {
                return false;
            }
        }

        if (__tryGetUnderlyingVariable(this, 0, name, out result))
        {
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public RuntimeContext GetGlobalContext()
    {
        RuntimeContext currentContext = this;

        while (currentContext.parent != null)
        {
            currentContext = currentContext.parent;
        }

        return currentContext;
    }

    public RuntimeContext GetCurrentScopeContext()
    {
        return this.parent ?? this;
    }

    public EvaluationResult Evaluate()
    {
        for (int i = 0; i < compilationResult.tokens.Length; i++)
        {
            Expression.EvaluateToken(this, ref compilationResult.tokens[i]);
        }
        return Result;
    }
}
