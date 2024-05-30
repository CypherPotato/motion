using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an <see cref="Atom"/> type.
/// </summary>
public enum AtomType
{
    /// <summary>
    /// Represents an null, undefined or empty atom.
    /// </summary>
    Null,

    /// <summary>
    /// Represents an <see cref="Atom"/> which holds an string value on it.
    /// </summary>
    StringLiteral,

    /// <summary>
    /// Represents an <see cref="Atom"/> which holds an character literal on it.
    /// </summary>
    CharacterLiteral,

    /// <summary>
    /// Represents an <see cref="Atom"/> which holds an numeric value on it.
    /// </summary>
    NumberLiteral,

    /// <summary>
    /// Represents an <see cref="Atom"/> which holds an boolean value on it.
    /// </summary>
    BooleanLiteral,

    // Other
    /// <summary>
    /// Represents an <see cref="Atom"/> which holds another children Atoms on it.
    /// </summary>
    Expression,

    /// <summary>
    /// Represents an array atom.
    /// </summary>
    Array,

    /// <summary>
    /// Represents an <see cref="Atom"/> which holds an symbol text.
    /// </summary>
    Symbol,

    /// <summary>
    /// Represents an keyword <see cref="Atom"/>.
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents an operator <see cref="Atom"/>.
    /// </summary>
    Operator
}
