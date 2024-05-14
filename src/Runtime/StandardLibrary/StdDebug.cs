using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdDebug : IMotionLibrary
{
    public string? Namespace => "dbg";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("members", atom =>
        {
            List<string> members = new List<string>();
            members.AddRange(atom.Context.Methods.Keys);
            members.AddRange(atom.Context.UserFunctions.Keys);
            members.AddRange(atom.Context.Variables.Keys);
            members.AddRange(atom.Context.Constants.Keys);

            members.Sort();
            return members.ToArray();
        });
        context.Methods.Add("aliases", atom =>
        {
            var aliases = atom.Context.Aliases.Keys;
            Array.Sort(aliases);
            return aliases;
        });
        context.Methods.Add("dump", atom =>
        {
            var obj = atom.GetAtom(1).Nullable()?.GetObject();
            if (obj is null)
            {
                return "NIL";
            }
            else if (obj is IEnumerable ie)
            {
                var enumerator = ie.GetEnumerator();
                StringBuilder sb = new StringBuilder();
                while (enumerator.MoveNext())
                {
                    sb.Append("- ");
                    sb.AppendLine(enumerator.Current?.ToString() ?? "NIL");
                }
                return sb.ToString();
            }
            else
            {
                return obj.ToString() ?? "NIL";
            }
        });
    }
}
