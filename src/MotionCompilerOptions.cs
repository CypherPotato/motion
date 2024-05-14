using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion;

/// <summary>
/// Represents an set of compiler options for the Motion compiler.
/// </summary>
public class MotionCompilerOptions
{
    /// <summary>
    /// Gets or sets an boolean indicating if the motion code can use (net:*) methods used for creating
    /// .NET runtime methods and members.
    /// </summary>
    public bool AllowDotNetInvoke { get; set; } = false;

    /// <summary>
    /// Gets or sets an boolean indicating if instructions without parenthesis should be allowed at the first
    /// level of the code.
    /// </summary>
    public bool AllowInlineDeclarations { get; set; } = false; 

    /// <summary>
    /// Gets or sets an collection of <see cref="IMotionLibrary"/> used to compile this code.
    /// </summary>
    public ICollection<IMotionLibrary> Libraries { get; set; } = new List<IMotionLibrary>();
}
