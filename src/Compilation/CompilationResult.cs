using Motion.Parser;
using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents the results of a Motion code compilation.
/// </summary>
public sealed class CompilationResult
{
    internal AtomBase[] tokens;

    /// <summary>
    /// Gets the specified <see cref="CompilerOptions"/> used to compile this result.
    /// </summary>
    public CompilerOptions Options { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the compilation was successful.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets any error that occurred during compilation, or null if the compilation was successful.
    /// </summary>
    public MotionException? Error { get; private set; }

    internal CompilationResult(AtomBase[] tokens, bool success, MotionException? error, CompilerOptions options)
    {
        this.tokens = tokens;
        Success = success;
        Error = error;
        Options = options;
    }

    /// <summary>
    /// Creates an execution context for the compiled Motion code.
    /// </summary>
    /// <returns>An execution context that can be used to execute the compiled code.</returns>
    /// <exception cref="MotionException">Thrown if the compilation was not successful.</exception>
    public Runtime.ExecutionContext CreateContext()
    {
        if (!Success) throw Error!;
        var context = Runtime.ExecutionContext.CreateBaseContext(this);

        context.ImportLibrary(new Runtime.StandardLibrary.StdCommon());
        context.ImportLibrary(new Runtime.StandardLibrary.StdString());
        context.ImportLibrary(new Runtime.StandardLibrary.StdMath());
        context.ImportLibrary(new Runtime.StandardLibrary.StdRandom());
        context.ImportLibrary(new Runtime.StandardLibrary.StdCType());

        if (Options.Features.HasFlag(CompilerFeature.EnableConsoleMethods))
            context.ImportLibrary(new Runtime.StandardLibrary.StdConsole());

        foreach (IMotionLibrary lib in Options.Libraries)
        {
            context.ImportLibrary(lib);
        }

        return context;
    }
}
