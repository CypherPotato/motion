using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an helper class which contains functions to use with <see cref="IMotionLibrary"/> objects.
/// </summary>
public static class LibraryHelper
{
    /// <summary>
    /// Creates an <see cref="MotionMethod"/> from the specified delegate void.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="MotionException"></exception>
    public static MotionMethod Create(Delegate method)
    {
        return (atom) => LibraryConverter(method, atom);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static object? LibraryConverter(Delegate method, Atom atom)
    {
        var firstChild = atom.GetAtom(0);
        var methodInfo = method.GetMethodInfo();
        int paramOffset = 0;
        List<object?> parameterObjects = new List<object?>();
        ParameterInfo[] arguments = methodInfo.GetParameters();

        if (arguments.Length == 1
            && arguments[0].ParameterType == typeof(Atom)
            && string.Compare(arguments[0].Name, "self", true) == 0)
        {
            return method.DynamicInvoke(new object?[] { atom });
        }

        object?[]? paramsArrayInstance = null;
        int paramsIndex = -1;
        object?[] inAtoms = new object[atom.ItemCount - 1];
        int requiredParams = 0;

        for (int i = 0; i < arguments.Length; i++)
        {
            var param = arguments[i];

            if (param.ParameterType == typeof(Atom) && param.Name?.Equals("self", StringComparison.CurrentCultureIgnoreCase) == true)
            {
                if (i != 0)
                {
                    throw new ArgumentException("The Self atom parameter must be the first parameter of the delegate.");
                }

                parameterObjects.Add(atom);
                paramOffset++;
            }
            else if (param.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                paramsIndex = i;
                if (i < inAtoms.Length)
                {
                    paramsArrayInstance = new object?[inAtoms.Length - i];
                }
                else
                {
                    paramsArrayInstance = Array.Empty<object?>();
                }
                break;
            }
            else if (!arguments[i].IsOptional)
            {
                requiredParams++;
            }
        }

        if (inAtoms.Length < requiredParams)
        {
            throw new MotionException($"this method requires at least {requiredParams} parameters, but got {inAtoms.Length} instead.", firstChild);
        }
        else if (paramsIndex == -1 && inAtoms.Length > requiredParams)
        {
            throw new MotionException($"too many arguments for the method \"{firstChild.GetSymbol()}\".\nthis method only expects {requiredParams} parameters, but got {inAtoms.Length} instead.", firstChild);
        }

        for (int i = 0; i < inAtoms.Length; i++)
        {
            if (paramsIndex >= 0 && i >= paramsIndex)
            {
                var at = atom.GetAtom(i + 1);
                object? result = at.Nullable()?.GetObject();

                // is it the last parameter, and is the last parameter an parameter array, and is the object
                // an collection?
                if (result is ICollection icol && i == paramsIndex && i == inAtoms.Length - 1)
                {
                    if (paramsArrayInstance!.Length < icol.Count)
                    {
                        Array.Resize(ref paramsArrayInstance, icol.Count);
                    }
                    icol.CopyTo(paramsArrayInstance, 0);
                }
                else
                {
                    paramsArrayInstance![i - paramsIndex] = result;
                }
            }
            else
            {
                if (paramsIndex >= 0 && paramOffset + i >= paramsIndex)
                {
                    continue;
                }

                var argType = arguments[paramOffset + i].ParameterType;
                var at = atom.GetAtom(i + 1);
                object? result;

                if (argType == typeof(Symbol))
                {
                    result = new Symbol(at.GetSymbol());
                }
                else if (argType == typeof(Atom))
                {
                    result = at;
                }
                else
                {
                    result = at.Nullable()?.GetObject();
                    if (result is not null && result.GetType().IsAssignableTo(argType) == false)
                    {
                        throw new MotionException($"Cannot convert type {result.GetType().FullName} at argument {i + 1} to {argType.FullName}. Are you missing a cast?", at);
                    }
                }

                parameterObjects.Add(result);
            }
        }

        if (paramsIndex >= 0)
        {
            parameterObjects.Add(paramsArrayInstance);
        }

        return method.DynamicInvoke(parameterObjects.ToArray());
    }
}
