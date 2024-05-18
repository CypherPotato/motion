using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    object? Coalesce(params object?[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] is not null)
                return values[i];
        }
        return null;
    }
}
