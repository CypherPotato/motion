using System.Numerics;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComKeys : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("increment", Increment);
        context.Methods.Add("type-of", NGetType);
        context.Methods.Add("exit", Exit);
        context.Methods.Add("coalesce", Coalesce);
        context.Methods.Add("whennil", WhenNil);
    }

    object? WhenNil(Atom self)
    {
        var v = self.GetAtom(1);

        if (v.Type == AtomType.Symbol && !self.Context.IsSymbolDefined(v.GetSymbol()))
        {
            ;
        }
        else
        {
            object? o = v.Nullable()?.GetObject();
            if (self.HasKeyword("or-empty"))
            {
                if (!string.IsNullOrEmpty(o?.ToString()))
                    return o;
            }
            else if (o is not null)
            {
                return o;
            }
        }

        if (self.ItemCount == 3)
        {
            return self.GetAtom(2).Nullable()?.GetObject();
        }
        else
        {
            return null;
        }
    }

    dynamic Increment(Atom self, Symbol name, dynamic value)
    {
        var bag = self.Context.GetScope(ExecutionContextScope.Function).Variables;
        var o = bag.Get(name.Contents);
        o += value;
        bag.Set(name.Contents, o);

        return o;
    }

    Type NGetType(object value)
    {
        return value.GetType();
    }

    object? Exit(object? value)
    {
        throw new MotionExitException(value);
    }

    object? Coalesce(Atom self)
    {
        foreach (Atom t in self.GetReader())
        {
            var value = t.Nullable()?.GetObject();
            if (value is not null)
            {
                return value;
            }
        }
        return null;
    }
}
