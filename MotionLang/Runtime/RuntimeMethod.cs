using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MotionLang.Interpreter;

namespace MotionLang.Runtime;

public delegate EvaluationResult RuntimeMethod(InvocationContext context);