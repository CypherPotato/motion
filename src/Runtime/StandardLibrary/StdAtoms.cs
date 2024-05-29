using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdAtoms : IMotionLibrary
{
    public string? Namespace => "atom";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("eval", Eval);
        context.Methods.Add("nullable", (Atom obj) => obj.Nullable());

        context.Methods.Add("eval-string", (Atom obj) => obj.GetString());

        context.Methods.Add("eval-bool", (Atom obj) => obj.GetBoolean());

        context.Methods.Add("eval-byte", (Atom obj) => obj.GetByte());
        context.Methods.Add("eval-sbyte", (Atom obj) => obj.GetSByte());

        context.Methods.Add("eval-int16", (Atom obj) => obj.GetInt16());
        context.Methods.Add("eval-int32", (Atom obj) => obj.GetInt32());
        context.Methods.Add("eval-int64", (Atom obj) => obj.GetInt64());

        context.Methods.Add("eval-uint16", (Atom obj) => obj.GetUInt16());
        context.Methods.Add("eval-uint32", (Atom obj) => obj.GetUInt32());
        context.Methods.Add("eval-uint64", (Atom obj) => obj.GetUInt64());

        context.Methods.Add("eval-double", (Atom obj) => obj.GetDouble());
        context.Methods.Add("eval-single", (Atom obj) => obj.GetFloat());

        context.Methods.Add("eval-symbol", (Atom obj, string keyword) => obj.GetSymbol());

        context.Methods.Add("child-by-keyword", (Atom obj, string keyword) => obj.GetAtom(keyword));
        context.Methods.Add("child-by-index", (Atom obj, int index) => obj.GetAtom(index));
        context.Methods.Add("children-list", (Atom obj) => obj.GetAtoms());
        context.Methods.Add("children-count", (Atom obj) => obj.ItemCount);
        context.Methods.Add("type", (Atom obj) => obj.Type);

        context.Methods.Add("has-keyword", (Atom obj, string keyword) => obj.HasKeyword(keyword));
    }

    object? Eval(Atom obj)
    {
        return obj.Nullable()?.GetObject();
    }
}
