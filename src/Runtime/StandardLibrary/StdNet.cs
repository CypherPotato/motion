using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdNet : IMotionLibrary
{
    public string? Namespace => "net";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("create-instance", atom =>
        {
            string target = atom.GetAtom(1).GetString();

            List<object?> args = new List<object?>();

            if (atom.ItemCount > 2)
            {
                for (int i = 2; i < atom.ItemCount; i++)
                {
                    args.Add(atom.GetAtom(i).Nullable()?.GetObject());
                }
            }

            Type? creatingTypename = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetType(target, false, true);
            if (creatingTypename == null)
            {
                throw new ArgumentException($"The type '{target}' couldn't be found.");
            }

            object result = Activator.CreateInstance(creatingTypename, args)!;
            return result;
        });
        context.Methods.Add("invoke-method", atom =>
        {
            object target = atom.GetAtom(1).GetObject();
            string methodName = atom.GetAtom(2).GetSymbol();

            List<object?> args = new List<object?>();

            if (atom.ItemCount > 3)
            {
                for (int i = 3; i < atom.ItemCount; i++)
                {
                    args.Add(atom.GetAtom(i).Nullable()?.GetObject());
                }
            }
            Type[] argTypes = args.Select(o => o?.GetType() ?? typeof(Object)).ToArray();

            BindingFlags state = atom.HasKeyword("static") ? BindingFlags.Static : BindingFlags.Instance;

            Type targetType = target.GetType();
            MethodInfo? method = targetType.GetMethod(methodName,
                BindingFlags.Public |
                state |
                BindingFlags.IgnoreCase, argTypes);
            
            if (method == null)
            {
                throw new ArgumentException($"'{targetType.FullName}' doens't contains an public method definition for '{methodName}' that accepts " +
                    $"({string.Join(", ", argTypes.Select(t => t.Name))}).");
            }

            object? value = method.Invoke(target, args.ToArray());
            return value;
        });
        context.Methods.Add("invoke-property", atom =>
        {
            object target = atom.GetAtom(1).GetObject();
            string propName = atom.GetAtom(2).GetSymbol();

            BindingFlags state = atom.HasKeyword("static") ? BindingFlags.Static : BindingFlags.Instance;

            Type targetType = target.GetType();
            PropertyInfo? method = targetType.GetProperty(propName,
                BindingFlags.Public |
                state |
                BindingFlags.IgnoreCase);

            if (method == null)
            {
                throw new ArgumentException($"'{targetType.FullName}' doens't contains an public property definition for '{propName}'.");
            }

            object? value = method.GetValue(target);
            return value;
        });
    }
}
