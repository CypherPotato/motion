using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComDeclarators : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("defvar", DefineVariable);
        context.Methods.Add("defconstant", DefineConstant);
        context.Methods.Add("defalias", DefineAlias);
        context.Methods.Add("set", SetVariable);
        context.Methods.Add("let", LetBlock);
        context.Methods.Add("with", WithBlock);
        context.Methods.Add("use", UseBlock);
    }

    object? DefineAlias(Atom self, Symbol name, Symbol reference)
    {
        self.Context.GetScope(ExecutionContextScope.Global)
            .Aliases.Add(name.Contents, reference.Contents);

        return true;
    }

    object? DefineVariable(Atom self, Symbol name, object? value)
    {
        self.Context.GetScope(ExecutionContextScope.Function)
            .Variables.Add(name.Contents, value);

        return value;
    }

    object? SetVariable(Atom self, Symbol name, object? value)
    {
        self.Context.GetScope(ExecutionContextScope.Function)
            .Variables.Set(name.Contents, value);

        return value;
    }

    object? DefineConstant(Atom self, Symbol name, object? value)
    {
        self.Context.GetScope(ExecutionContextScope.Function)
            .Constants.Add(name.Contents, value);

        return value;
    }

    object? LetBlock(Atom self)
    {
        self.EnsureExactItemCount(3);

        var varBlock = self.GetAtom(1);
        var expBlock = self.GetAtom(2);

        var varBlockAtoms = varBlock.GetAtoms();

        foreach (Atom varBlockAtom in varBlockAtoms)
        {
            var key = varBlockAtom.GetAtom(0).GetSymbol();
            var value = varBlockAtom.GetAtom(1).Nullable()?.GetObject();

            self.Context.Variables.Set(key, value);
        }

        return expBlock.Nullable()?.GetObject();
    }

    object? WithBlock(Atom self, object? value, Symbol name, Atom expression)
    {
        self.Context.Variables.Set(name.Contents, value);
        return expression.Nullable()?.GetObject();
    }

    object? UseBlock(Atom self, IDisposable value, Symbol name, Atom expression)
    {
        try
        {
            self.Context.Variables.Set(name.Contents, value);
            return expression.Nullable()?.GetObject();
        }
        finally
        {
            value.Dispose();
        }
    }
}
