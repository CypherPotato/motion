using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

public class MethodCollection : MotionCollection<MotionMethod>
{
    public MethodCollection(ExecutionContext context, bool canInsert, bool canEdit) : base(context, canInsert, canEdit)
    {
    }

    public void Add(Delegate method)
    {
        string name = method.GetMethodInfo().Name;
        Add(name, method);
    }

    public void Add(string name, Delegate method)
    {
        base.Add(name, LibraryHelper.Create(method));
    }
}
