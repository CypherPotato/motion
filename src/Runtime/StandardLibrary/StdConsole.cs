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
        context.Aliases.Add("write", "console:write");

        context.Methods.Add("write-line", (atom) =>
        {
            if (atom.ItemCount == 1)
            {
                Console.WriteLine();
            }
            else if (atom.ItemCount == 2)
            {
                Console.WriteLine(atom.GetAtom(1).Nullable()?.GetObject());
            }
            else
            {
                object?[] args = atom.GetAtoms()
                    .Skip(2)
                    .Select(a => a.GetObject())
                    .ToArray();
                Console.WriteLine(atom.GetAtom(1).GetString(), args);
            }
            return null;
        });
        context.Methods.Add("write", (atom) =>
        {
            atom.EnsureExactItemCount(2);
            Console.Write(atom.GetAtom(1).Nullable()?.GetObject());
            return null;
        });
    }
}
