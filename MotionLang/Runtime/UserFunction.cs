using MotionLang.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Runtime;

public class UserFunction
{
    public string[] Arguments { get; set; }
    public Expression Body { get; set; }

    public UserFunction(String[] arguments, Expression body)
    {
        Arguments = arguments;
        Body = body;
    }
}
