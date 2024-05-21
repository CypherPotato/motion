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
                completionItemDescriptionPaneBackground: AnsiColor.Rgb(30, 30, 30),
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
        foreach (var variable in context.Variables.Keys)
            motionPromptCallback.AutocompleteTerms.Add((variable, AnsiColor.Rgb(207, 247, 255), $"Variable {variable}"));
        foreach (var constant in context.Constants.Keys)
            motionPromptCallback.AutocompleteTerms.Add((constant, AnsiColor.Rgb(194, 218, 255), $"Constant {constant}"));
        foreach (var method in context.Methods.Keys)
            motionPromptCallback.AutocompleteTerms.Add((method, AnsiColor.Rgb(255, 233, 194), $"Method {method}"));
        foreach (var func in context.UserFunctions.Keys)
            motionPromptCallback.AutocompleteTerms.Add((func, AnsiColor.Rgb(242, 255, 204), $"User function {func}"));
        foreach (var alias in context.Aliases.Keys)
            motionPromptCallback.AutocompleteTerms.Add((alias, AnsiColor.Rgb(255, 209, 245), $"Alias {alias}"));

        StringBuilder buildingExpression = new StringBuilder();
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

            buildingExpression.AppendLine(data);
            string code = buildingExpression.ToString();

            bool hadFileImportingErrors = false;


            if (hadFileImportingErrors)
                continue;

            if (Compiler.GetParenthesisIndex(code) <= 0)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                buildingExpression.Clear();

                try
                {
                    var result = context.Run(code);
                    sw.Stop();

                    Console.WriteLine(result.LastOrDefault()?.ToString()?.ReplaceLineEndings());

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
