using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser.V2;

record struct TextPosition
{
    public int column;
    public int line;
    public int index;
}