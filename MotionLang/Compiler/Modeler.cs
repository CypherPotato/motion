using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Compiler;

internal class Modeler
{
   public static void ModelLexcalGraphs(Provider.CompilationResult compilationResult)
    {
        var compileContext = compilationResult.CreateContext();
        compileContext.isCompileTime = true;
        compileContext.Evaluate();
    }
}
