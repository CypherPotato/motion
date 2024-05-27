using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Parser;

struct TextInterpreterSnapshot
{
    public int Line;
    public int Column;
    public int Index;
    public string? Filename;
    public int Length;
    public bool Initialized = false;

    public TextInterpreterSnapshot(int line, int column, int position, int length, string? filename)
    {
        Initialized = true;
        Line = line;
        Column = column;
        Length = length;
        Index = position;
        Filename = filename;
    }
}