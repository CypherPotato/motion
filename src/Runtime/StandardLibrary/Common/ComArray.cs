using System.Collections;
using System.Dynamic;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComArray : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("make-list", MakeList);
        context.Methods.Add("make-array", MakeArray);
        context.Methods.Add("make-object", MakeObject);

        context.Methods.Add("dotimes", DoTimes);
        context.Methods.Add("map", Map);
        context.Methods.Add("while", While);
        context.Methods.Add("length", Length);
        context.Methods.Add("aref", Aref);

        context.Methods.Add("lpush", Lpush);
    }

    IList Lpush(IList item, object? value)
    {
        item.Add(value);
        return item;
    }

    ArrayList MakeList(Atom self)
    {
        ArrayList arr = new ArrayList();
        for (int i = 1; i < self.ItemCount; i++)
        {
            arr.Add(self.GetAtom(i).Nullable()?.GetObject());
        }

        return arr;
    }

    object? MakeObject(Atom self)
    {
        var obj = new MotionObject();
        foreach(string word in self.Keywords)
        {
            obj.Add(word, self.GetAtom(word).Nullable()?.GetObject());
        }
        return obj;
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

    int Length(ICollection objects)
    {
        return objects.Count;
    }

    dynamic? Aref(dynamic dynamics, dynamic? key)
    {
        return dynamics[key];
    }
}
