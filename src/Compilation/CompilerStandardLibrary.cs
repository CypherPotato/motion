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
    /// Represents the common Motion functions.
    /// </summary>
    StdCommon = 1 << 1,

    /// <summary>
    /// Represents the string functions.
    /// </summary>
    StdString = 1 << 2,

    /// <summary>
    /// Represents the math functions.
    /// </summary>
    StdMath = 1 << 3,

    /// <summary>
    /// Represents the random functions.
    /// </summary>
    StdRandom = 1 << 4,

    /// <summary>
    /// Represents the standard console library.
    /// </summary>
    StdConsole = 1 << 5,

    /// <summary>
    /// Represents the standard convert type library.
    /// </summary>
    StdConvertType = 1 << 6,

    /// <summary>
    /// Represents all the standard libraries.
    /// </summary>
    All = 
        StdCommon
        | StdString
        | StdMath
        | StdRandom
        | StdConsole
        | StdConvertType,

    /// <summary>
    /// Represents all safe standard libraries.
    /// </summary>
    AllSafe =
        StdCommon
        | StdString
        | StdMath
        | StdRandom
        | StdConvertType,
}
