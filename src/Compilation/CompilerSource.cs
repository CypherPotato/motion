using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents an Motion source code.
/// </summary>
public sealed class CompilerSource
{
    /// <summary>
    /// Gets or sets the file name of this Motion source code.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets or sets the Motion source code contents.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Creates an new <see cref="CompilerSource"/> instance with specified parameters.
    /// </summary>
    /// <param name="filename">The file name of this Motion source code.</param>
    /// <param name="source">The Motion source code contents.</param>
    public CompilerSource(string? filename, string source)
    {
        Filename = filename;
        Source = source;
    }
}