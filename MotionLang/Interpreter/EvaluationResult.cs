using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Interpreter;

public class EvaluationResult
{
    public static readonly EvaluationResult Void = new EvaluationResult() { IsVoid = true, Result = null };

    public bool IsVoid { get; internal set; }
    public object? Result { get; internal set; }

    internal EvaluationResult() { }
    public EvaluationResult(object? result)
    {
        IsVoid = false;
        Result = result;
    }
}
