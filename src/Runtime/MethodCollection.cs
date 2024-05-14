using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an <see cref="MotionCollection{TValue}"/> of runtime methods.
/// </summary>
public class MethodCollection : MotionCollection<MotionMethod>
{
    internal MethodCollection(ExecutionContext context, bool canInsert, bool canEdit) : base(context, canInsert, canEdit)
    {
    }

    /// <summary>
    /// Adds the specified delegate method into this collection, using it's method name.
    /// </summary>
    /// <param name="method">The delegate method.</param>
    public void Add(Delegate method)
    {
        string name = method.GetMethodInfo().Name;
        Add(name, method);
    }

    /// <summary>
    /// Adds the specified delegate method into this collection with the specified name.
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <param name="method">The method delegate.</param>
    public void Add(string name, Delegate method)
    {
        base.Add(name, LibraryHelper.Create(method));
    }
}
