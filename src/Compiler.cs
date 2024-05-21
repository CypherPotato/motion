using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Runtime;
using System.Reflection;
using Motion.Compilation;

namespace Motion;

/// <summary>
/// Provides methods for compiling and running Motion code.
/// </summary>
public static class Compiler
{
    /// <summary>
    /// Gets the Motion version text.
    /// </summary>
    public const string MotionVersion = "v0.1-alpha-2";

    /// <summary>
    /// Parses and constructs a sequence of objects representing atoms and their types from the input text. This method does not throw any errors.
    /// </summary>
    /// <param name="code">The Motion code to analyse.</param>
    public static SyntaxItem[] AnalyzeSyntax(string code)
    {
        var linter = new Analyzer(code);
        linter.Lint();

        return linter.GetSyntaxItems();
    }

    /// <summary>
    /// Compiles and runs the specified Motion code into their respective atomic values. This method throws an <see cref="MotionException"/> if the
    /// specified code has syntax errors or if caught an exception while running the code.
    /// </summary>
    /// <param name="code">The Motion code to run.</param>
    /// <param name="options">Optional. Defines the <see cref="CompilerOptions"/> options to the compiler.</param>
    /// <returns>An array of results of the returned atom values.</returns>
    public static object?[] Evaluate(string code, CompilerOptions? options = null)
    {
        var result = Compile(code, options);
        if (!result.Success)
        {
            throw result.Error!;
        }

        return result.CreateContext().Evaluate();
    }

    /// <summary>
    /// Compiles the specified list of <see cref="CompilerSource"/> into a representation that can be executed by the Motion interpreter.
    /// </summary>
    /// <param name="sources">An list of <see cref="CompilerSource"/> to compile.</param>
    /// <param name="options">Optional. Defines the <see cref="CompilerOptions"/> options to the compiler.</param>
    /// <returns>A <see cref="CompilationResult"/> object containing the results of the compilation, including any errors or warnings.</returns>
    public static CompilationResult Compile(IEnumerable<CompilerSource> sources, CompilerOptions? options = null)
    {
        CompilerOptions _options = options ?? new CompilerOptions();
        try
        {
            List<AtomBase> buildingAtoms = new List<AtomBase>();

            foreach (var source in sources)
            {
                AtomBase[] tokens = new Tokenizer(source.Source, source.Filename, _options).Tokenize().ToArray();
                buildingAtoms.AddRange(tokens);
            }

            return new CompilationResult(buildingAtoms.ToArray(), true, null, _options);
        }
        catch (MotionException ex)
        {
            return new CompilationResult(Array.Empty<AtomBase>(), false, ex, _options);
        }
    }

    /// <summary>
    /// Compiles the specified Motion code into a representation that can be executed by the Motion interpreter.
    /// </summary>
    /// <param name="code">The Motion code to compile.</param>
    /// <param name="options">Optional. Defines the <see cref="CompilerOptions"/> options to the compiler.</param>
    /// <returns>A <see cref="CompilationResult"/> object containing the results of the compilation, including any errors or warnings.</returns>
    public static CompilationResult Compile(string code, CompilerOptions? options = null)
    {
        CompilerOptions _options = options ?? new CompilerOptions();
        try
        {
            AtomBase[] tokens = new Tokenizer(code, null, _options).Tokenize().ToArray();
            return new CompilationResult(tokens, true, null, _options);
        }
        catch (MotionException ex)
        {
            return new CompilationResult(Array.Empty<AtomBase>(), false, ex, _options);
        }
    }

    /// <summary>
    /// Creates an default, empty <see cref="ExecutionContext"/> with no code.
    /// </summary>
    /// <param name="options">Optional. Defines the <see cref="CompilerOptions"/> options to the compiler.</param>
    /// <returns></returns>
    public static Runtime.ExecutionContext CreateEmptyContext(CompilerOptions? options = null)
    {
        var result = new CompilationResult(Array.Empty<AtomBase>(), true, null, options ?? new CompilerOptions());
        return result.CreateContext();
    }

    /// <summary>
    /// Checks whether the Motion code has all pairs of atoms properly closed. This method does not evaluate the code.
    /// </summary>
    /// <param name="code">The Motion code to verify.</param>
    /// <returns>Returns true if the code is properly enclosed.</returns>
    public static int GetParenthesisIndex(string code)
    {
        Sanitizer.SanitizeCode(code, out int parenthesisIndex, out _);
        return parenthesisIndex;
    }
}