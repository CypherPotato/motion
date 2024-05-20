using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComFunctions : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("defun", Defun);
        context.Methods.Add("import", Import);
        context.Methods.Add("get-trace", GetTrace);
    }

    string GetTrace(Atom self)
    {
        return self.Context.traceWriter.ToString();
    }

    void Import(Atom self, Symbol name)
    {
        self.Context.Global.UsingStatements.Add(name.Contents);
    }

    void Defun(Atom self)
    {
        string name = self.GetAtom(1).GetSymbol();
        string[] args;
        string? documentation;
        Atom body;

        if (self.ItemCount == 5)
        {
            // has docs
            args = self.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
            documentation = self.GetAtom(3).GetString();
            body = self.GetAtom(4);
        }
        else if (self.ItemCount == 4)
        {
            args = self.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
            documentation = null;
            body = self.GetAtom(3);
        }
        else
        {
            throw new InvalidOperationException($"defun requires at least 3 parameters. got {self.ItemCount}.");
        }

        MotionUserFunction func = new MotionUserFunction(args, documentation, body._ref);
        self.Context.GetScope(ExecutionContextScope.Global).UserFunctions.Add(name, func);
    }
}
