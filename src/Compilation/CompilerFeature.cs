using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents special flags and features to the Motion compiler.
/// </summary>
[Flags]
public enum CompilerFeature
{
    /// <summary>
    /// Indicates that the compile should allow running code which the first
    /// level aren't enclosed by an parenthesis block.
    /// </summary>
    AllowParenthesislessCode = 1 << 1
}
