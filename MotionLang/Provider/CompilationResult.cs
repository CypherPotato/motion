using MotionLang.Compiler;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Provider;

public class CompilationResult
{
    public Dictionary<string, object?> Constants { get; private set; }
    public Dictionary<string, UserFunction> UserFunctions { get; private set; }
    public bool Success { get; private set; }
    public MotionException? Error { get; private set; }
    public RuntimeModule Runtime { get; private set; }

    internal Token[] tokens;

    public RuntimeContext CreateContext()
    {
        return new RuntimeContext(this, null, ContextScope.Global);
    }

    internal CompilationResult(bool success, MotionException? error, RuntimeModule runtime, Token[] tokens)
    {
        this.Success = success;
        this.Error = error;
        this.tokens = tokens;
        this.UserFunctions = new Dictionary<string, UserFunction>(StringComparer.OrdinalIgnoreCase);
        this.Constants = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        this.Runtime = runtime;

        Modeler.ModelLexcalGraphs(this);
    }
}
