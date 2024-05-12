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

    internal MotionUserFunction(string[] arguments, string? documentation, AtomBase body)
    {
        Arguments = arguments;
        Documentation = documentation;
        this.body = body;
    }

    internal object? Invoke(AtomBase callingExpression, ExecutionContext context)
    {
        if (Math.Max(callingExpression.Children.Length - 1, 0) != Arguments.Length)
        {
            throw new MotionException($"Expected {Arguments.Length} parameters. Got {callingExpression.Children.Length - 1}.", callingExpression.Location, null);
        }

        for (int i = 0; i < Arguments.Length; i++)
        {
            string key = Arguments[i];
            object? value = context.EvaluateTokenItem(callingExpression.Children[i + 1], callingExpression);
            context.Variables.Set(key, value);
        }

        return context.EvaluateTokenItem(body, AtomBase.Undefined);
    }
}