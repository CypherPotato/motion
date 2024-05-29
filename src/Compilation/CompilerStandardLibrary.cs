using Motion.Runtime.StandardLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents standard built-in Motion libraries.
/// </summary>
[Flags]
public enum CompilerStandardLibrary
{
    /// <summary>
    /// Represents the standard console library.
    /// </summary>
    StdConsole = 1 << 1,

    /// <summary>
    /// Represents all the standard libraries.
    /// </summary>
    All =
        StdConsole
}
