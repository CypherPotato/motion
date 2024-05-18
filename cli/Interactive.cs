using Motion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCLI;
internal static class Interactive
{
    public static void Init()
    {
        Motion.Runtime.ExecutionContext? lastContext = null;
        StringBuilder buildingExpression = new StringBuilder();
        while (true)
        {
            Console.Write(">>> ");
            string? data = Console.ReadLine();

            if (data == "/reset")
            {
                lastContext = null;
                continue;
            }
            else if (data == "/exit")
            {
                break;
            }
            else if (data == "/clear")
            {
                Console.Clear();
                continue;
            }

            buildingExpression.AppendLine(data);

            string code = buildingExpression.ToString();

            if (Compiler.GetParenthesisIndex(code) <= 0)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                buildingExpression.Clear();
                var compilationResult = Compiler.Compile(code, new MotionCompilerOptions()
                {
                    AllowDotNetInvoke = true,
                    AllowInlineDeclarations = true
                });

                if (compilationResult.Success)
                {
                    try
                    {
                        var context = compilationResult.CreateContext();
                        context.ImportState(lastContext);

                        var result = context.Evaluate();
                        sw.Stop();

                        lastContext = context;

                        if (result.Length == 0)
                        {
                            Console.WriteLine(result);
                        }
                        else
                        {
                            foreach (var item in result)
                            {
                                Console.WriteLine("<- " + item);
                            }
                        }

                        if (Program.Verbose)
                        {
                            Console.Write($"\n<- {result?.GetType().FullName ?? "(NIL)"} in {sw.ElapsedMilliseconds}ms");
                            Console.WriteLine();
                        }

                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        DumpError(ex);
                    }
                }
                else
                {
                    DumpError(compilationResult.Error!);
                }
            }
        }
    }

    static void DumpError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        if (ex is MotionException error)
        {
            Console.WriteLine($"at line {error.Line}, col {error.Column}: {error.Message}");
            Console.WriteLine();
            Console.WriteLine(error.LineText);
            Console.WriteLine(new string(' ', Math.Max(0, error.Column - 1)) + new string('^', error.Length));
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"error: {ex.Message}");
            Console.WriteLine();
        }
        Console.ResetColor();
    }
}
