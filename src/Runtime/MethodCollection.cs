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
public class MethodCollection : MotionCollection<Delegate>
{
    internal MethodCollection(ExecutionContext context, bool canInsert, bool canEdit) : base(context, canInsert, canEdit)
    {
    }
}