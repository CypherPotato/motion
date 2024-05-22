using Motion;
using PrettyPrompt.Highlighting;
using PrettyPrompt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MotionCLI.Program;
using Motion.Compilation;
using Motion.Runtime;

namespace MotionCLI;

internal static class Interactive
{
    public static async Task Init()
    {
        Console.WriteLine($"Motion Messaging Client [Cli. {Program.ClientVersionString}/Lang. {Motion.Compiler.MotionVersion}]");
        Console.WriteLine("To get help, type /help.\n");

        var motionPromptCallback = new MotionPromptCallback();
        await using var prompt = new Prompt(
            persistentHistoryFilepath: "./history-file",
            callbacks: motionPromptCallback,
            configuration: new PromptConfiguration(
                prompt: new FormattedString(Environment.UserDomainName + "> "),
                selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                selectedTextBackground: AnsiColor.Rgb(20, 61, 102)));

        CompilerOptions options = new CompilerOptions()
        {
            Features =
                  CompilerFeature.AllowParenthesislessCode
                | CompilerFeature.EnableConsoleMethods
                | CompilerFeature.TraceUserFunctionsCalls
                | CompilerFeature.TraceUserFunctionsVariables
        };

        Motion.Runtime.ExecutionContext context;
        if (ImportedFiles.Length > 0)
        {
            List<CompilerSource> sources = new List<CompilerSource>();

            foreach (string file in Program.ImportedFiles)
            {
                string fpath = Path.GetFullPath(file);
                if (!File.Exists(fpath))
                {
                    Console.WriteLine($"error: the specified importing file {file} couldn't be found.");
                    break;
                }
                sources.Add(new CompilerSource(fpath, File.ReadAllText(fpath)));
            }

            var compilerResult = Compiler.Compile(sources, options);
            if (!compilerResult.Success)
            {
                DumpError(compilerResult.Error!);
                return;
            }

            context = compilerResult.CreateContext();

            // try to evaluate compiled code
            try
            {
                context.Evaluate();
            }
            catch (Exception ex)
            {
                DumpError(ex);
                return;
            }
        }
        else
        {
            context = Compiler.CreateEmptyContext(options);
        }

        // get auto complete items
        foreach (AtomicInformation<object?> variable in context.Variables)
            motionPromptCallback.AutocompleteTerms.Add((variable.Name, Program.Theme.MenuVariable, AutoMenuFunctions.BuildVariableAutomenu(variable)));
        foreach (var constant in context.Constants)
            motionPromptCallback.AutocompleteTerms.Add((constant.Name, Program.Theme.MenuConstant, AutoMenuFunctions.BuildConstantAutomenu(constant)));
        foreach (var method in context.Methods)
            motionPromptCallback.AutocompleteTerms.Add((method.Name, Program.Theme.MenuMethod, AutoMenuFunctions.BuildMethodAutomenu(method)));
        foreach (AtomicInformation<MotionUserFunction> func in context.UserFunctions)
            motionPromptCallback.AutocompleteTerms.Add((func.Name, Program.Theme.MenuUserFunction, AutoMenuFunctions.BuildUserFunctionAutomenu(func)));
        foreach (var alias in context.Aliases.Keys)
            motionPromptCallback.AutocompleteTerms.Add((alias, Program.Theme.MenuAlias, $"Alias {alias}"));

        while (true)
        {
            var response = await prompt.ReadLineAsync();
            var data = response.Text;
            
            if (data == "/exit")
            {
                break;
            }
            else if (data == "/clear")
            {
                Console.Clear();
                continue;
            }
            else if (data == "/rebuild")
            {
                Console.Clear();
                await Init();
                return;
            }
            else if (data == "/help")
            {
                Console.WriteLine("""
                    Command-line client usage:

                    MOTIONCLI [...options]

                    Available server options:

                        -e, --endpoint              Specifies the Motion server endpoint.
                        -u, --auth                  Specifies the Motion server authentication string.

                    Available local options:

                        -f, --file                  Specifies an Motion code file to include to the
                                                    compiler.
                        -v, --verbose               Enables verbose output.

                    """);

                continue;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                var result = context.Run(data);
                sw.Stop();

                Console.WriteLine(result.LastOrDefault()?.ToString()?.ReplaceLineEndings());

                if (Program.Verbose)
                {
                    Console.Write($"\n<- {result?.GetType().FullName ?? "(NIL)"} in {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine();
                }

            }
            catch (Exception ex)
            {
                DumpError(ex);
            }
        }
    }

    static void DumpError(Exception ex)
    {
        if (ex is MotionException error)
        {
            MotionException.DumpErrorMessage(error);
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"error: {ex.Message}");
            Console.WriteLine();
        }
    }
}
