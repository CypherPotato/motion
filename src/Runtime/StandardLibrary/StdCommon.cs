using Motion.Runtime.StandardLibrary.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Motion.Runtime.StandardLibrary;

class StdCommon : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Constants.Add("motion-version", Compiler.MotionVersion);
        new ArithmeticOperators().ApplyMembers(context);
        new BooleanOperators().ApplyMembers(context);
        new ComDeclarators().ApplyMembers(context);
        new ComArray().ApplyMembers(context);
        new ComFunctions().ApplyMembers(context);
        new ComKeys().ApplyMembers(context);
    }
}
