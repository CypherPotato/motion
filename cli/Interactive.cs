﻿using LightJsonMotionLib;
using Motion;
using Motion.Compilation;
using Motion.Runtime;
using PrettyPrompt;
using PrettyPrompt.Highlighting;
using System.Diagnostics;
using System.Reflection;
using static MotionCLI.Program;

namespace MotionCLI;

internal static class Interactive
{
    public static async Task Init()
    {
        Dictionary<string, string> fileInputs = new Dictionary<string, string>();
        Console.WriteLine($"Motion Messaging Client [Cli. {Program.ClientVersionString}/Lang. {Motion.Compiler.MotionVersion}]");
        Console.WriteLine("To get help, type /help.\n");

        // resolve references
        List<IMotionLibrary> references = new List<IMotionLibrary>()
        {
            new LHtml(),
            new LJson()
        };

        if (Program.References.Length > 0)
        {
            foreach (string f in Program.References)
            {
                Assembly fromRef = Assembly.LoadFrom(f);
                Type[] types = fromRef.GetTypes()
                    .Where(r => r.GetCustomAttribute<LibraryEntrypointAttribute>() != null)
                    .ToArray();

                IMotionLibrary[] instances = types
                    .Select(Activator.CreateInstance)
                    .Select(o => (IMotionLibrary)o!)
                    .ToArray();

                foreach (IMotionLibrary instance in instances)
                    references.Add(instance);
            }
        }

        var motionPromptCallback = new MotionPromptCallback();
        await using var prompt = new Prompt(
            persistentHistoryFilepath: "./history-file",
            callbacks: motionPromptCallback,
            configuration: new PromptConfiguration(
                prompt: new FormattedString(Environment.UserName + " -> "),
                selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                selectedTextBackground: AnsiColor.Rgb(20, 61, 102)));

        motionPromptCallback.Parent = prompt;

        CompilerOptions options = new CompilerOptions()
        {
            Features =
                  CompilerFeature.AllowParenthesislessCode,

            Libraries = references,

            StandardLibraries = CompilerStandardLibrary.All,
            ExposeCLR = true
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

                string code = File.ReadAllText(file);

                fileInputs.Add(file, code);
                sources.Add(CompilerSource.FromCode(code, file));
            }

            var compilerResult = Compiler.Compile(sources, options);
            if (!compilerResult.Success)
            {
                var error = compilerResult.Error!;
                string? fromFile = error.Filename is null ? null : fileInputs[error.Filename];
                DumpError(fromFile, error);
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
                string? fileText = null;
                if (ex is MotionException mex)
                {
                    fileText = mex.Filename is null ? null : fileInputs[mex.Filename];
                }
                DumpError(fileText, ex);

                return;
            }
        }
        else
        {
            context = Compiler.CreateEmptyContext(options);
        }

        context.Variables.Set("$last-result", null);

        // get auto complete items
        void UpdateAutocompleteItems()
        {
            motionPromptCallback.AutocompleteTerms.Clear();

            foreach (var variable in context.Variables)
                motionPromptCallback.AutocompleteTerms.Add((variable.Key, Program.Theme.MenuVariable, AutoMenuFunctions.BuildVariableAutomenu(variable)));
            ;
            foreach (var constant in context.Constants)
                motionPromptCallback.AutocompleteTerms.Add((constant.Key, Program.Theme.MenuConstant, AutoMenuFunctions.BuildConstantAutomenu(constant)));
            ;
            foreach (var method in context.Methods)
                motionPromptCallback.AutocompleteTerms.Add((method.Key, Program.Theme.MenuMethod, AutoMenuFunctions.BuildMethodAutomenu(method)));
            ;
            foreach (var func in context.UserFunctions)
                motionPromptCallback.AutocompleteTerms.Add((func.Key, Program.Theme.MenuUserFunction, AutoMenuFunctions.BuildUserFunctionAutomenu(func)));
            ;
            foreach (var alias in context.Aliases.Keys)
                motionPromptCallback.AutocompleteTerms.Add((alias, Program.Theme.MenuAlias, $"Alias {alias}"));
        }

        while (true)
        {
            UpdateAutocompleteItems();
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

                context.Variables.Set("$last-result", result);
                Console.WriteLine(result?.ToString()?.ReplaceLineEndings());

                if (Program.Verbose)
                {
                    Console.Write($"\n<- {result?.GetType().FullName ?? "(NIL)"} in {sw.ElapsedMilliseconds}ms");
                    Console.WriteLine();
                }

            }
            catch (Exception ex)
            {
                string? fileText = null;
                if (ex is MotionException mex)
                {
                    fileText = mex.Filename is null ? data : fileInputs[mex.Filename];
                }

                DumpError(fileText, ex);
            }
        }
    }

    static void DumpError(string? code, Exception ex)
    {
        if (ex is MotionException error)
        {
            MotionException.DumpErrorMessage(code, error);
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"error: {ex.Message}");
            Console.WriteLine();
        }
    }
}
