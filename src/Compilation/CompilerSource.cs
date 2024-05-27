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

    public static CompilerSource FromCode(string code)
    {
        return new CompilerSource() { Source = new StringReader(code) };
    }

    public static CompilerSource FromCode(string code, string? fileName)
    {
        return new CompilerSource() { Source = new StringReader(code), Filename = fileName };
    }

    public static CompilerSource FromFile(string fileName, Encoding? encoding = null)
    {
        return new CompilerSource() { Source = new StreamReader(fileName, encoding ?? Encoding.UTF8), Filename = fileName };
    }

    public static CompilerSource FromStream(Stream s, string? fileName = null, Encoding? encoding = null)
    {
        return new CompilerSource() { Source = new StreamReader(s, encoding ?? Encoding.UTF8), Filename = fileName };
    }
}