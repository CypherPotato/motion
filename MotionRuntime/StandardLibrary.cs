using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotionRuntime;

public class StandardLibrary
{
    internal List<RuntimeMethod> Methods { get; set; } = new List<RuntimeMethod>();

    public void AddMethod(RuntimeMethod method)
    {
        Methods.Add(method);
    }

    public StandardLibrary()
    {
        ImportModules(typeof(Export.Arrays));
        ImportModules(typeof(Export.Console));
        ImportModules(typeof(Export.Math));
        ImportModules(typeof(Export.String));
        ImportModules(typeof(Export.Variables));
        ImportModules(typeof(Export.Common));
        ImportModules(typeof(Export.Op));
    }

    public RuntimeModule CreateRuntime()
    {
        return new RuntimeModule(Methods.ToArray());
    }

    void ImportModules(Type type)
    {
        MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (MethodInfo method in methods)
        {
            Methods.Add((RuntimeMethod)Delegate.CreateDelegate(typeof(RuntimeMethod), method));
        }
    }
}