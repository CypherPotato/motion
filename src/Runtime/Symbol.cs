using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an symbol in Motion code.
/// </summary>
public struct Symbol
{
    /// <summary>
    /// Gets the contents of this symbol.
    /// </summary>
    public string Contents { get; set; }

    internal Symbol(string content)
    {
        Contents = content;
    }
}
