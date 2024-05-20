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
        StringBuilder buildingExpression = new StringBuilder();
        while (true)
        {
            Console.Write(">>> ");
            string? data = Console.ReadLine();

            if (data == "/exit")
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

            bool hadFileImportingErrors = false;
            List<MotionSource> sources = new List<MotionSource>();
            foreach (string file in Program.ImportedFiles)
            {
                string fpath = Path.GetFullPath(file);
                if (!File.Exists(fpath))
                {
                    Console.WriteLine($"error: the specified importing file {file} couldn't be found.");
                    hadFileImportingErrors = true;
                    break;
                }
                sources.Add(new MotionSource(fpath, File.ReadAllText(fpath)));
            }

            if (hadFileImportingErrors) 
                continue;

            if (Compiler.GetParenthesisIndex(code) <= 0)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                buildingExpression.Clear();

                sources.Add(new MotionSource(null, code));

                var compilationResult = Compiler.Compile(sources, new MotionCompilerOptions()
                {
                    Features =
                          CompilerFeature.AllowParenthesislessCode
                        | CompilerFeature.EnableConsoleMethods
                        | CompilerFeature.TraceUserFunctionsCalls
                        | CompilerFeature.TraceUserFunctionsVariables
                });

                if (compilationResult.Success)
                {
                    try
                    {
                        var context = compilationResult.CreateContext();

                        var result = context.Evaluate();
                        sw.Stop();

                        Console.WriteLine(result.LastOrDefault());

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
