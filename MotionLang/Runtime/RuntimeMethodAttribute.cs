using MotionLang.Compiler;

namespace MotionLang.Runtime;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RuntimeMethodAttribute : Attribute
{
    public string MethodName { get; set; }

    public RuntimeMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
