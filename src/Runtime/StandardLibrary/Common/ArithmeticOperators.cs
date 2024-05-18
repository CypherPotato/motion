using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;
internal class ArithmeticOperators : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("+", Sum);
        context.Methods.Add("-", Sub);
        context.Methods.Add("*", Mult);
        context.Methods.Add("/", Div);
    }

    dynamic Sum(params dynamic[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException("At least one operand is required.");

        dynamic carry = nums[0];
        for (int i = 1; i < nums.Length; i++)
            carry *= nums[i];

        return carry;
    }

    dynamic Sub(params dynamic[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException("At least one operand is required.");

        dynamic carry = nums[0];
        for (int i = 1; i < nums.Length; i++)
            carry *= nums[i];

        return carry;
    }

    dynamic Mult(params dynamic[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException("At least one operand is required.");

        dynamic carry = nums[0];
        for (int i = 1; i < nums.Length; i++)
            carry *= nums[i];

        return carry;
    }

    dynamic Div(params dynamic[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException("At least one operand is required.");

        dynamic carry = nums[0];
        for (int i = 1; i < nums.Length; i++)
            carry /= nums[i];

        return carry;
    }
}
