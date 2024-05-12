using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

public enum AtomType
{
    Null,

    // data types
    StringLiteral,
    NumberLiteral,
    BooleanLiteral,

    // Other
    Expression,

    Symbol,
    Keyword,
    Operator
}
