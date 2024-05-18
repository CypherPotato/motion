using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComArray : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("make-array", MakeArray);
        context.Methods.Add("dotimes", DoTimes);
        context.Methods.Add("map", Map);
        context.Methods.Add("while", While);
        context.Methods.Add("array-count", ArrayCount);
        context.Methods.Add("aref", Aref);
    }

    object?[] MakeArray(Atom self)
    {
        object?[] arr = new object?[self.ItemCount - 1];
        for (int i = 1; i < self.ItemCount; i++)
        {
            arr[i - 1] = self.GetAtom(i).Nullable()?.GetObject();
        }

        return arr;
    }

    ArrayList DoTimes(Atom self, Atom iteratorExp, Atom body)
    {
        ArrayList result = new ArrayList();
        var iteratorVar = iteratorExp.GetAtom(0).GetSymbol();
        var iteratorMax = iteratorExp.GetAtom(1).GetInt32();

        for (int i = 0; i < iteratorMax; i++)
        {
            self.Context.SetVariable(iteratorVar, i);
            result.Add(body.Nullable()?.GetObject());
        }

        return result;
    }

    ArrayList Map(Atom self, Atom iteratorExp, Atom body)
    {
        ArrayList result = new ArrayList();

        var iteratorArr = (IEnumerable)iteratorExp.GetAtom(0).GetObject();
        var iteratorVar = iteratorExp.GetAtom(1).GetSymbol();

        foreach (var item in iteratorArr)
        {
            self.Context.SetVariable(iteratorVar, item);
            result.Add(body.Nullable()?.GetObject());
        }

        return result;
    }

    ArrayList While(Atom self, Atom condition, Atom statement)
    {
        ArrayList result = new ArrayList();

        while (condition.GetBoolean() == true)
        {
            result.Add(statement.Nullable()?.GetObject());
        }

        return result;
    }

    int ArrayCount(ICollection objects)
    {
        return objects.Count;
    }

    dynamic? Aref(dynamic dynamics, dynamic? key)
    {
        return dynamics[key];
    }
}
