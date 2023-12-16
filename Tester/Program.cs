using MotionLang.Compiler;
using MotionLang.Interpreter;
using MotionLang.Provider;
using MotionLang.Runtime;
using System.Diagnostics;

namespace Tester;

internal class Program
{
    static void Main(string[] args)
    {
        string code = File.ReadAllText(@"C:\Users\gscat\Desktop\test2.lsp");

        var runtime = new MotionRuntime.StandardLibrary();
        var module = MotionProvider.Compile(code, runtime.CreateRuntime());

        if (!module.Success)
        {
            DumpError(module.Error!);
        }

        try
        {
            var context = module.CreateContext();
            var result = context.Evaluate().Result;
        }
        catch (Exception ex)
        {
            DumpError(ex);
        }

        Thread.Sleep(-1);
    }

    static void DumpError(Exception result)
    {
        if (result is MotionException cex)
        {
            Console.WriteLine($"at line {cex.Line}, column {cex.Column}: {cex.Message}\n");
            Console.WriteLine($"\t{cex.LineText.Trim()}");
            Console.WriteLine($"\t{new string(' ', cex.Column - 1)}{new string('^', cex.Length)}");
        }
        else
        {
            Console.WriteLine(result.Message);
        }
        Environment.Exit(1);
    }
}
