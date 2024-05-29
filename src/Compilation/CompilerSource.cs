using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents an Motion source code.
/// </summary>
public sealed class CompilerSource : IDisposable
{
    /// <summary>
    /// Gets or sets the file name of this Motion source code.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets or sets the Motion source code contents.
    /// </summary>
    public required TextReader Source { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Source?.Dispose();
    }

    /// <summary>
    /// Creates an new <see cref="CompilerSource"/> instance from the specified code snippet.
    /// </summary>
    /// <param name="code">The input source code.</param>
    /// <returns></returns>
    public static CompilerSource FromCode(string code)
    {
        return new CompilerSource() { Source = new StringReader(code) };
    }

    /// <summary>
    /// Creates an new <see cref="CompilerSource"/> instance from the specified code snippet.
    /// </summary>
    /// <param name="code">The input source code.</param>
    /// <param name="fileName">The file name of the source code.</param>
    /// <returns></returns>
    public static CompilerSource FromCode(string code, string? fileName)
    {
        return new CompilerSource() { Source = new StringReader(code), Filename = fileName };
    }

    /// <summary>
    /// Creates an new <see cref="CompilerSource"/> instance from the specified file.
    /// </summary>
    /// <param name="fileName">The file path.</param>
    /// <param name="encoding">The encoding used to decode the file contents.</param>
    public static CompilerSource FromFile(string fileName, Encoding? encoding = null)
    {
        return new CompilerSource() { Source = new StreamReader(fileName, encoding ?? Encoding.UTF8), Filename = fileName };
    }

    /// <summary>
    /// Creates an new <see cref="CompilerSource"/> instance from the specified stream.
    /// </summary>
    /// <param name="s">The input stream.</param>
    /// <param name="fileName">The file name to the <see cref="CompilerSource"/>.</param>
    /// <param name="encoding">The encoding used to decode the input stream.</param>
    /// <returns></returns>
    public static CompilerSource FromStream(Stream s, string? fileName = null, Encoding? encoding = null)
    {
        return new CompilerSource() { Source = new StreamReader(s, encoding ?? Encoding.UTF8), Filename = fileName };
    }
}