using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser;
using Motion.Runtime;
using Motion;
using System.Reflection;

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
    /// Compiles and runs the specified Motion code into their respective atomic values. This method throws an <see cref="MotionException"/> if the
    /// specified code has syntax errors or if caught an exception while running the code.
    /// </summary>
    /// <param name="code">The Motion code to run.</param>
    /// <param name="options">Optional. Defines the <see cref="MotionCompilerOptions"/> options to the compiler.</param>
    /// <returns>An array of results of the returned atom values.</returns>
    public static object?[] Evaluate(string code, MotionCompilerOptions? options = null)
    {
        var result = Compile(code, options);
        if (!result.Success)
        {
            throw result.Error!;
        }

        return result.CreateContext().Evaluate();
    }

    /// <summary>
    /// Compiles the specified Motion code into a representation that can be executed by the Motion interpreter.
    /// </summary>
    /// <param name="code">The Motion code to compile.</param>
    /// <param name="options">Optional. Defines the <see cref="MotionCompilerOptions"/> options to the compiler.</param>
    /// <returns>A <see cref="CompilerResult"/> object containing the results of the compilation, including any errors or warnings.</returns>
    public static CompilerResult Compile(string code, MotionCompilerOptions? options = null)
    {
        MotionCompilerOptions _options = options ?? new MotionCompilerOptions();
        try
        {
            AtomBase[] tokens = new Tokenizer(code, _options).Tokenize().ToArray();
            return new CompilerResult(tokens, true, null, _options);
        }
        catch (MotionException ex)
        {
            return new CompilerResult(Array.Empty<AtomBase>(), false, ex, _options);
        }
    }

    /// <summary>
    /// Checks whether the Motion code has all pairs of atoms properly closed. This method does not evaluate the code.
    /// </summary>
    /// <param name="code">The Motion code to verify.</param>
    /// <returns>Returns true if the code is properly enclosed.</returns>
    public static int GetParenthesisIndex(string code)
    {
        Sanitizer.SanitizeCode(code, out int parenthesisIndex);
        return parenthesisIndex;
    }
}

/// <summary>
/// Represents the results of a Motion code compilation.
/// </summary>
public sealed class CompilerResult
{
    private AtomBase[] _tokens;

    /// <summary>
    /// Gets the specified <see cref="MotionCompilerOptions"/> used to compile this result.
    /// </summary>
    public MotionCompilerOptions Options { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the compilation was successful.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets any error that occurred during compilation, or null if the compilation was successful.
    /// </summary>
    public MotionException? Error { get; private set; }

    internal CompilerResult(AtomBase[] tokens, bool success, MotionException? error, MotionCompilerOptions options)
    {
        _tokens = tokens;
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
        var context = Runtime.ExecutionContext.CreateBaseContext(_tokens);

        context.ImportLibrary(new Runtime.StandardLibrary.StdCommon());
        context.ImportLibrary(new Runtime.StandardLibrary.StdMath());
        context.ImportLibrary(new Runtime.StandardLibrary.StdString());
        context.ImportLibrary(new Runtime.StandardLibrary.StdConsole());
        context.ImportLibrary(new Runtime.StandardLibrary.StdRandom());
        context.ImportLibrary(new Runtime.StandardLibrary.StdDebug());

        if (Options.AllowDotNetInvoke)
        {
            context.ImportLibrary(new Runtime.StandardLibrary.StdNet());
        }

        foreach (IMotionLibrary lib in Options.Libraries)
        {
            context.ImportLibrary(lib);
        }

        return context;
    }

    /// <summary>
    /// Gets an array of <see cref="MotionSymbolInformation"/> of this compiled result.
    /// </summary>
    public MotionSymbolInformation[] GetSymbolsInformation()
    {
        var ctx = CreateContext();

        List<MotionSymbolInformation> info = new List<MotionSymbolInformation>();
        info.AddRange(ctx.Variables.Keys.Select(v => new MotionSymbolInformation(MotionSymbolInformationType.Variable, v)));
        info.AddRange(ctx.Constants.Keys.Select(v => new MotionSymbolInformation(MotionSymbolInformationType.Constant, v)));
        info.AddRange(ctx.Methods.Keys.Select(v => new MotionSymbolInformation(MotionSymbolInformationType.Method, v)));
        info.AddRange(ctx.UserFunctions.Keys.Select(v => new MotionSymbolInformation(MotionSymbolInformationType.UserFunction, v)));

        return info.ToArray();
    }

    /// <summary>
    /// Emits an <see cref="MotionTreeItem"/> syntax tree of this compiled result.
    /// </summary>
    /// <returns></returns>
    public MotionTreeItem BuildSyntaxTree()
    {
        MotionTreeItem _BuildPairFor(AtomBase t)
        {
            string key = t.ToString();

            if (t.Children.Length > 0)
            {
                List<MotionTreeItem> children = new List<MotionTreeItem>();
                foreach (AtomBase s in t.Children)
                {
                    children.Add(_BuildPairFor(s));
                }
                return new MotionTreeItem(key, children.ToArray(), t);
            }
            else
            {
                return new MotionTreeItem(key, Array.Empty<MotionTreeItem>(), t);
            }
        }

        List<MotionTreeItem> rootNodes = new List<MotionTreeItem>();
        foreach (AtomBase s in _tokens)
        {
            rootNodes.Add(_BuildPairFor(s));
        }

        return new MotionTreeItem("root", rootNodes.ToArray(), AtomBase.Undefined);
    }
}

/// <summary>
/// Represents an class which contains informations about an specific symbol in an Motion code.
/// </summary>
public class MotionSymbolInformation
{
    /// <summary>
    /// Gets the symbol type.
    /// </summary>
    public MotionSymbolInformationType Type { get; }

    /// <summary>
    /// Gets the symbol name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates an new instance of the <see cref="MotionSymbolInformation"/> class with given parameters.
    /// </summary>
    /// <param name="type">The symbol type.</param>
    /// <param name="name">The symbol name.</param>
    public MotionSymbolInformation(MotionSymbolInformationType type, string name)
    {
        Type = type;
        Name = name;
    }
}

/// <summary>
/// Represents an symbol type.
/// </summary>
public enum MotionSymbolInformationType
{
    /// <summary>
    /// Represents an symbol which is defined within an variable value.
    /// </summary>
    Variable,

    /// <summary>
    /// Represents an symbol which is defined within an constant value.
    /// </summary>
    Constant,

    /// <summary>
    /// Represents an symbol which points to an runtime method.
    /// </summary>
    Method,

    /// <summary>
    /// Represents an symbol which points to an Motion-defined function.
    /// </summary>
    UserFunction
}

/// <summary>
/// Represents an syntax tree of an Motion code Atom.
/// </summary>
public class MotionTreeItem
{
    /// <summary>
    /// Gets the raw, unformatted contents of the atom.
    /// </summary>
    public string RawContents { get; }

    /// <summary>
    /// Gets the visual representation of this atom.
    /// </summary>
    public string Contents { get; }

    /// <summary>
    /// Gets the children <see cref="MotionTreeItem"/> items of this atom.
    /// </summary>
    public MotionTreeItem[] Children { get; }

    /// <summary>
    /// Gets the <see cref="AtomType"/> which represents this Atom.
    /// </summary>
    public AtomType Type { get; }

    /// <summary>
    /// Gets the line position of this Atom.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column position of this Atom.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the absolute index of this Atom.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the length of this Atom.
    /// </summary>
    public int Length { get; }

    internal MotionTreeItem(string contents, IEnumerable<MotionTreeItem> children, AtomBase at)
    {
        RawContents = at.Content?.ToString() ?? "";
        Contents = contents;
        Children = children.ToArray();
        Type = Atom.AtomTypeFromTokenType(at.Type);
        Line = at.Location.Line;
        Column = at.Location.Column;
        Position = at.Location.Position;
        Length = at.Location.Length;
    }
}