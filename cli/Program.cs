using Motion;
using Motion.Runtime;
using System.Diagnostics;
using System.Text;

namespace MotionCLI;

internal class Program
{
    public static bool EnableColors = true;
    public static bool Verbose = true;

    static void Main(string[] args)
    {
        var cmdParser = new CommandLine.CommandLineParser(args);

        EnableColors = !cmdParser.IsDefined("no-colors");
        Verbose = cmdParser.IsDefined("verbose", 'v');

        InitInteractive();
        Console.ResetColor();
    }

    static void InitInteractive()
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
                var compilationResult = Compiler.Compile(code, new CompilerOptions()
                {
                    AllowDotNetInvoke = true,
                    Libraries = new[] { new Vec2Lib() }
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

                        if (Verbose)
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

record struct Vec2
{
    public float X;
    public float Y;

    public void Echo()
    {
        Console.WriteLine(this);
    }
}

public class Vec2Lib : IMotionLibrary
{
    public string? Namespace => "vec2";

    public void ApplyMembers(Motion.Runtime.ExecutionContext context)
    {
        context.Methods.Add("create", (float x, float y) => new Vec2() { X = x, Y = y });
        context.Methods.Add("get-x", (Vec2 vec) => vec.X);
        context.Methods.Add("get-y", (Vec2 vec) => vec.Y);
    }
}