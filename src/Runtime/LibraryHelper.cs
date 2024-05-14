using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        return (atom) =>
        {
            var methodInfo = method.GetMethodInfo();
            ParameterInfo[] arguments = methodInfo.GetParameters();

            object?[] inAtoms = new object[atom.ItemCount - 1];
            int requiredParams = 0;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].IsOptional)
                {
                    requiredParams++;
                }
            }

            if (inAtoms.Length < requiredParams)
            {
                throw new ArgumentException($"This method requires at least {requiredParams} parameters. Got {inAtoms.Length} instead.");
            }
            else if (inAtoms.Length > arguments.Length)
            {
                throw new ArgumentException($"Too many arguments. This method only expects {arguments.Length} with required and optional parameters, but got {inAtoms.Length} instead.");
            }

            for (int i = 0; i < inAtoms.Length; i++)
            {
                var argType = arguments[i].ParameterType;
                var at = atom.GetAtom(i + 1);
                object? result = at.Nullable()?.GetObject();

                if (result is not null && result.GetType().IsAssignableTo(argType) == false)
                {
                    throw new MotionException($"Cannot convert type {result.GetType().FullName} at argument ^{i + 1} to {argType.FullName}. Are you missing a cast?", at);
                }

                inAtoms[i] = result;
            }

            return method.DynamicInvoke(inAtoms);
        };
    }
}
