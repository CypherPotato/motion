using Motion.Parser;
using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Whe using AOT compilation, the user should use EnumExport.Create instead it's constructor.")]
    public Runtime.ExecutionContext CreateContext()
    {
        if (!Success) throw Error!;
        var context = Runtime.ExecutionContext.CreateBaseContext(this);

        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdCommon))
            context.ImportLibrary(new Runtime.StandardLibrary.StdCommon());
        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdString))
            context.ImportLibrary(new Runtime.StandardLibrary.StdString());
        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdMath))
            context.ImportLibrary(new Runtime.StandardLibrary.StdMath());
        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdRandom))
            context.ImportLibrary(new Runtime.StandardLibrary.StdRandom());
        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdConvertType))
            context.ImportLibrary(new Runtime.StandardLibrary.StdCType());
        if (Options.StandardLibraries.HasFlag(CompilerStandardLibrary.StdConsole))
            context.ImportLibrary(new Runtime.StandardLibrary.StdConsole());

        foreach (IMotionLibrary lib in Options.Libraries)
        {
            context.ImportLibrary(lib);
        }

        foreach (EnumExport export in Options.EnumExports)
        {
            var names = Enum.GetNames(export.ExportType);
            var values = Enum.GetValues(export.ExportType);

            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i];
                object v = values.GetValue(i)!;

                string prefix = (export.Alias ?? export.ExportType.Name) + ".";
                context.SetConstant(prefix + n, v);
            }
        }

        return context;
    }
}
