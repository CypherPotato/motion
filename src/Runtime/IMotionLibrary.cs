using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Provides an function to embedding runtime methods in an Motion runtime.
/// </summary>
public interface IMotionLibrary
{
    /// <summary>
    /// Gets the namespace text which is applied to all methods and constants of this
    /// <see cref="IMotionLibrary"/>.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Apply the library methods into an runtime context.
    /// </summary>
    /// <param name="context">The runtime context where the Motion code is running.</param>
    public void ApplyMembers(ExecutionContext context);
}
