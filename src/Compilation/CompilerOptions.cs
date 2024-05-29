using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents an set of compiler options for the Motion compiler.
/// </summary>
public class CompilerOptions
{
    /// <summary>
    /// Gets or sets special compiler flags and features to the Motion compiler.
    /// </summary>
    public CompilerFeature Features { get; set; }

    /// <summary>
    /// Gets or sets an collection of <see cref="IMotionLibrary"/> used to compile this code.
    /// </summary>
    public ICollection<IMotionLibrary> Libraries { get; set; } = new List<IMotionLibrary>();

    /// <summary>
    /// Gets or sets an collection of <see cref="EnumExport"/> used to export enum members.
    /// </summary>
    public ICollection<EnumExport> EnumExports { get; set; } = new List<EnumExport>();
}
