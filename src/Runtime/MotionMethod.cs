using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

/// <summary>
/// Represents an dynamic defined Motion method.
/// </summary>
/// <param name="atom">Represents the <see cref="Atom"/> where this method is being called.</param>
public delegate object? MotionMethod(Atom atom);

/// <summary>
/// Represents an function defined in the Motion code.
/// </summary>
public sealed class MotionUserFunction
{
    private AtomBase body;

    /// <summary>
    /// Gets an ordered array of arguments names of this Motion function.
    /// </summary>
    public string[] Arguments { get; }

    /// <summary>
    /// Gets the optional documentation of this Motion function.
    /// </summary>
    public string? Documentation { get; }

    /// <summary>
    /// Gets whether this method should evaluate arguments or invoke them as atoms.
    /// </summary>
    public bool EvaluateArguments { get; }

    internal MotionUserFunction(string[] arguments, string? documentation, AtomBase body, bool evaluateArguments)
    {
        Arguments = arguments;
        Documentation = documentation;
        this.body = body;
        EvaluateArguments = evaluateArguments;
    }

    internal object? Invoke(AtomBase callingExpression, ExecutionContext context)
    {
        var firstChild = callingExpression.Type == TokenType.Expression ?
            callingExpression.Children[0] : callingExpression;

        int atomCount = Math.Max(callingExpression.Children.Length - 1, 0);
        if (atomCount < Arguments.Length)
        {
            throw new MotionException($"this method requires at least {Arguments.Length} parameters, but got {atomCount} instead.", firstChild.Location, null);
        }
        else if (atomCount > Arguments.Length)
        {
            throw new MotionException($"too many arguments for the method \"{firstChild.Content}\".\nthis method only expects {Arguments.Length} parameters, but got {atomCount} instead.", firstChild.Location, null);
        }

        if (EvaluateArguments)
        {
            for (int i = 0; i < Arguments.Length; i++)
            {
                string key = Arguments[i];
                object? value = context.EvaluateTokenItem(callingExpression.Children[i + 1], callingExpression);
                context.CallingParameters.InternalSet(key, value);
            }

            return context.EvaluateTokenItem(body, AtomBase.Undefined);
        }
        else
        {
            void ReplAtomValue(ref AtomBase item, string name, in AtomBase newValue)
            {
                for (int i = 0; i < item.Children.Length; i++)
                {
                    ref AtomBase child = ref item.Children[i];
                    if (child.Type == TokenType.Symbol && string.Compare(child.Content?.ToString(), name, true) == 0)
                    {
                        string? k = child.Keyword;
                        child = newValue;
                        child.Keyword = k;
                    }
                    else if (child.Type == TokenType.Expression)
                    {
                        ReplAtomValue(ref child, name, newValue);
                    }
                }
            }

            AtomBase copy = body.Clone();
            for (int i = 0; i < Arguments.Length; i++)
            {
                string key = Arguments[i];
                ref AtomBase value = ref callingExpression.Children[i + 1];
                ReplAtomValue(ref copy, key, in value);
            }

            return context.EvaluateTokenItem(copy, AtomBase.Undefined);
        }
    }
}