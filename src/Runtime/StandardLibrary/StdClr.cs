using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

#pragma warning disable IL2067 
#pragma warning disable IL2070
#pragma warning disable IL2075

internal class StdClr : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("new", CreateInstance);
        context.Methods.Add("icall", InvokeStatic);
        context.Methods.Add("get-field", GetField);
        context.Methods.Add("set-field", SetField);
        context.Methods.Add("export-enum", ExportEnum);
    }

    public void ExportEnum(Atom self)
    {
        var type = self.GetAtom(1).GetObject<Type>();
        var alias = self.HasKeyword("as") ? self.GetAtom("as").GetSymbol() : null;

        TypeHelper.ExportEnum(type, alias, self.Context.Global);
    }

    public object? CreateInstance(Atom self, Type type, params object?[] args)
    {
        object? instance = Activator.CreateInstance(type, args);
        return instance;
    }

    public object? GetField(Atom self, object obj, Symbol name)
    {
        Type t = obj.GetType();

        BindingFlags flags;
        if (self.HasKeyword("static"))
        {
            flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;
        }
        else
        {
            flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        }

        FieldInfo? field = t.GetField(name.Contents, flags);

        if (field is null)
        {
            throw new MotionException($"'{t.FullName}' does not contain an field for '{name}'.", self.GetAtom(2));
        }

        return field.GetValue(obj);
    }

    public object? SetField(Atom self, object obj, Symbol name, object? newValue)
    {
        Type t = obj.GetType();

        BindingFlags flags;
        if (self.HasKeyword("static"))
        {
            flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;
        }
        else
        {
            flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        }

        FieldInfo? field = t.GetField(name.Contents, flags);

        if (field is null)
        {
            throw new MotionException($"'{t.FullName}' does not contain an field for '{name}'.", self.GetAtom(2));
        }

        field.SetValue(obj, newValue);

        return newValue;
    }

    public object? InvokeStatic(Atom self)
    {
        Type type = self.GetAtom(1).GetObject<Type>();
        string name = self.GetAtom(2).GetSymbol();

        var t = self._ref;

        List<object?> argValues = new List<object?>();
        List<Type?> typeHints = new List<Type?>(t.Children.Length);

        for (int i = 3; i < t.Children.Length; i++)
        {
            var child = t.Children[i];
            Type? childType = TypeHelper.ResolveTypeByAtom(ref child);
            typeHints.Add(childType);
            argValues.Add(self.GetAtom(i).Nullable()?.GetObject());
        }

        BindingFlags bflag =
              BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.IgnoreCase
            ;

        MethodInfo[] methods = type.GetMethods(bflag);
        MethodInfo? publicMethod = TypeHelper.FindMatchingMethodInfo(name, methods, typeHints, t.Children.Length == 3);

        if (publicMethod is null)
        {
            if (typeHints.Count == 0)
            {
                throw new MotionException($"'{type.FullName}' does not contain a public static method for '{name}'.", t.Children[1].Location, null);
            }
            else
            {
                throw new MotionException($"'{type.FullName}' does not contain a public static method for '{name}' that accepts the parameters:\n- {string.Join("- ", typeHints.Select(s => s.FullName + "\n"))}", t.Children[1].Location, null);
            }
        }

        return publicMethod.Invoke(null, argValues.ToArray());
    }
}
