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
    AllowParenthesislessCode = 1 << 1,

    /// <summary>
    /// Indicates that the runtime can trace user defined functions calls.
    /// </summary>
    TraceUserFunctionsCalls = 1 << 3,

    /// <summary>
    /// Indicates that the runtime can trace user runtime functions calls.
    /// </summary>
    TraceRuntimeFunctionsCalls = 1 << 4,

    /// <summary>
    /// Indicates that the runtime can trace variables values.
    /// </summary>
    TraceUserFunctionsVariables = 1 << 5
}
