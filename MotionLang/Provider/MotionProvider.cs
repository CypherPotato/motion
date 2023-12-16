using MotionLang.Compiler;
using MotionLang.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Provider;

public static class MotionProvider
{
    public static CompilationResult Compile(string code, RuntimeModule runtime)
    {
        try
        {
            var tokenizer = new Tokenizer(code);
            Token[] tokens = tokenizer.Tokenize().ToArray();
            return new CompilationResult(true, null, runtime, tokens);
        }
        catch (MotionException ex)
        {
            return new CompilationResult(false, ex, runtime, Array.Empty<Token>());
        }
    }
}
