using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion.Parser.V2;

namespace Motion.Parser;

internal class TextInterpreterExceptionManager
{
    public TextInterpreter Interpreter { get; set; }

    public TextInterpreterExceptionManager(TextInterpreter interpreter)
    {
        Interpreter = interpreter;
    }

    public MotionException UnknownExpression(string token, TextInterpreterSnapshot snapshot)
    {
        return new MotionException($"unexpected token: '{token}'.", snapshot, null);
    }

    public MotionException UnexpectedToken(string token)
    {
        return new MotionException($"unexpected token: '{token}'.", Interpreter.GetSnapshot(1), null);
    }

    public MotionException ExpectToken(string token)
    {
        return new MotionException($"'{token}' expected.", Interpreter.GetSnapshot(1), null);
    }

    public MotionException NotFinishedString()
    {
        return new MotionException("the input has ended before the reader could finish reading the code.", Interpreter.GetSnapshot(1), null);
    }
}
