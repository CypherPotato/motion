using Motion;
using Motion.Compilation;
using Motion.Runtime;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace MotionCLI;

internal class Program
{
    public static readonly string ClientVersionString = "v.0.1";

    public static bool Verbose = true;
    public static string? ServerEndpoint = null;
    public static string? ServerAuth = null;
    public static string[] ImportedFiles = null!;

    static async Task Main(string[] args)
    {
        var cmdParser = new CommandLine.CommandLineParser(args);

        Verbose = cmdParser.IsDefined("verbose", 'v');
        ServerEndpoint = cmdParser.GetValue("endpoint", 'e');
        ServerAuth = cmdParser.GetValue("auth", 'u');
        ImportedFiles = cmdParser.GetValues("file", 'f').ToArray();

        if (ServerEndpoint is null)
        {
            await Interactive.Init();
        }
        else
        {
            await ServerMessenger.Init();
        }
    }

    internal class MotionPromptCallback : PromptCallbacks
    {
        public List<(string Term, AnsiColor Color, string Description)> AutocompleteTerms { get; set; } = new();
        protected IReadOnlyCollection<FormatSpan> VisibleTokens { get; set; } = Array.Empty<FormatSpan>();


        protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
        {
            // demo completion algorithm callback
            // populate completions and documentation for autocompletion window
            var typedWord = text.AsSpan(spanToBeReplaced.Start, spanToBeReplaced.Length).ToString();

            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                AutocompleteTerms
                .Select(term =>
                    new CompletionItem(
                        replacementText: term.Term,
                        getExtendedDescription: _ => Task.FromResult(new FormattedString(term.Description)),
                        displayText: new FormattedString(term.Term, new ConsoleFormat(term.Color))
                    )
                )
                .ToArray()
            );
        }

        protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
        {
            List<FormatSpan> result = new List<FormatSpan>();
            SyntaxItem[] tree = Compiler.AnalyzeSyntax(text);

            ;

            for (int i = 0; i < tree.Length; i++)
            {
                var before = tree[Math.Max(i - 1, 0)];
                var item = tree[i];

                ConsoleFormat format = default;
                TextSpan textSpan = new TextSpan(item.Position, item.Length);

                if (item.Type is SyntaxItemType.BooleanLiteral or SyntaxItemType.Operator or SyntaxItemType.NullWord)
                {
                    format = new ConsoleFormat(AnsiColor.Rgb(92, 195, 250));
                }
                else if (item.Type is SyntaxItemType.StringLiteral)
                {
                    format = new ConsoleFormat(AnsiColor.Rgb(255, 102, 102));
                }
                else if (item.Type is SyntaxItemType.NumberLiteral)
                {
                    format = new ConsoleFormat(AnsiColor.Rgb(185, 250, 225));
                }
                else if (item.Type is SyntaxItemType.Symbol)
                {
                    if (before.Type == SyntaxItemType.ExpressionStart)
                    {
                        format = new ConsoleFormat(AnsiColor.Rgb(255, 233, 194));
                    }
                    else
                    {
                        format = new ConsoleFormat(AnsiColor.Rgb(216, 235, 240));
                    }
                }
                else if (item.Type is SyntaxItemType.Keyword)
                {
                    format = new ConsoleFormat(AnsiColor.Rgb(237, 207, 255));
                }
                else if (item.Type is SyntaxItemType.Comment)
                {
                    format = new ConsoleFormat(AnsiColor.Rgb(106, 173, 127));
                }
                else
                {
                    continue;
                }

                result.Add(new FormatSpan(textSpan, format));
            }

            VisibleTokens = result;
            return Task.FromResult<IReadOnlyCollection<FormatSpan>>(result);
        }
    }
}