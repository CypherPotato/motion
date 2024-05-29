using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdConsole : IMotionLibrary
{
    public string? Namespace => "console";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Aliases.Add("print", "console:write");
        context.Aliases.Add("println", "console:write-line");

        context.Methods.Add("write", Write);
        context.Methods.Add("write-line", WriteLine);
    }

    void Write(object? s) => Console.Write(s);
    void WriteLine(object? s) => Console.WriteLine(s);
}
