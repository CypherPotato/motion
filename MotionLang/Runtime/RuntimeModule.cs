using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Runtime;

public class RuntimeModule
{
    internal Dictionary<string, RuntimeMethod> runtimeMethods;
    internal Dictionary<string, RuntimeMethod> compTimeMethods;

    public RuntimeModule(RuntimeMethod[] methods)
    {
        runtimeMethods = new Dictionary<string, RuntimeMethod>(StringComparer.OrdinalIgnoreCase);
        compTimeMethods = new Dictionary<string, RuntimeMethod>(StringComparer.OrdinalIgnoreCase);

        compTimeMethods.Add("defun", Common.StandardRuntime.DefineFunction);
        compTimeMethods.Add("const", Common.StandardRuntime.Const);

        foreach (RuntimeMethod method in methods)
        {
            string? methodName = method.Method.Name;
            RuntimeMethodAttribute? attr = method.Method.GetCustomAttribute<RuntimeMethodAttribute>();

            if (attr != null)
            {
                methodName = attr.MethodName;
            }

            runtimeMethods.Add(methodName, method);
        }
    }
}
