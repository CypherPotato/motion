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

    public static int _smallerImportedFileIndex = 0;

    public static Theme Theme = new Theme();

    static async Task Main(string[] args)
    {
        var cmdParser = new CommandLine.CommandLineParser(args);

        Verbose = cmdParser.IsDefined("verbose", 'v');
        ServerEndpoint = cmdParser.GetValue("endpoint", 'e');
        ServerAuth = cmdParser.GetValue("auth", 'u');
        ImportedFiles = cmdParser.GetValues("file", 'f').ToArray();

        _smallerImportedFileIndex = ImportedFiles
            .Select(Path.GetFullPath)
            .Select(Path.GetDirectoryName)
            .Select(f => f?.Length ?? 0)
            .Min();


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
        public List<(string Term, ConsoleFormat Format, FormattedString Description)> AutocompleteTerms { get; set; } = new();
        protected IReadOnlyCollection<FormatSpan> VisibleTokens { get; set; } = Array.Empty<FormatSpan>();


        protected override Task<TextSpan> GetSpanToReplaceByCompletionAsync(string text, int caret, CancellationToken cancellationToken)
        {
            int num = caret;
            int num2 = num - 1;
            while (num2 >= 0 && IsWordCharacter(text[num2]))
            {
                num--;
                num2--;
            }

            if (num < 0)
            {
                num = 0;
            }

            int num3 = caret;
            for (int i = caret; i < text.Length && IsWordCharacter(text[i]); i++)
            {
                num3++;
            }

            return Task.FromResult(TextSpan.FromBounds(num, num3));

            bool IsWordCharacter(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ':' || c == '.' || c == '$' || c == '%';
        }

        protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
        {
            // demo completion algorithm callback
            // populate completions and documentation for autocompletion window
            var typedWord = text.AsSpan(spanToBeReplaced.Start, spanToBeReplaced.Length).ToString();

            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                AutocompleteTerms
                .Where(term => AutocompleteSuggestPredicate(term.Term, typedWord))
                .Select(term =>
                    new CompletionItem(
                        replacementText: term.Term,
                        getExtendedDescription: _ => Task.FromResult(term.Description),
                        displayText: new FormattedString(term.Term, term.Format)
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

                ConsoleFormat format;
                TextSpan textSpan = new TextSpan(item.Position, item.Length);

                if (item.Type is SyntaxItemType.BooleanLiteral or SyntaxItemType.Operator or SyntaxItemType.NullWord)
                {
                    format = Theme.WordLiteral;
                }
                else if (item.Type is SyntaxItemType.StringLiteral)
                {
                    format = Theme.StringLiteral;
                }
                else if (item.Type is SyntaxItemType.NumberLiteral)
                {
                    format = Theme.NumberLiteral;
                }
                else if (item.Type is SyntaxItemType.Symbol)
                {
                    if (before.Type == SyntaxItemType.ExpressionStart || i == 0)
                    {
                        format = Theme.FunctionName;
                    }
                    else
                    {
                        format = Theme.Symbol;
                    }
                }
                else if (item.Type is SyntaxItemType.Keyword)
                {
                    format = Theme.Keyword;
                }
                else if (item.Type is SyntaxItemType.Comment)
                {
                    format = Theme.Comment;
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

    static bool AutocompleteSuggestPredicate(string s, string t)
    {
        string S = new string(s.Where(char.IsLetterOrDigit).ToArray());
        string T = new string(t.Where(char.IsLetterOrDigit).ToArray());

        return S.Contains(T, StringComparison.CurrentCultureIgnoreCase) || ComputeLevenshteinDistance(S, T) <= 2;
    }

    static int ComputeLevenshteinDistance(string s, string t)
    {
        int n = s.Length; // length of s
        int m = t.Length; // length of t

        if (n == 0)
        {
            return m;
        }
        else if (m == 0)
        {
            return n;
        }

        int[] p = new int[n + 1]; //'previous' cost array, horizontally
        int[] d = new int[n + 1]; // cost array, horizontally
        int[] _d; //placeholder to assist in swapping p and d

        // indexes into strings s and t
        int i; // iterates through s
        int j; // iterates through t

        char t_j; // jth character of t

        int cost; // cost

        for (i = 0; i <= n; i++)
        {
            p[i] = i;
        }

        for (j = 1; j <= m; j++)
        {
            t_j = t[j - 1];
            d[0] = j;

            for (i = 1; i <= n; i++)
            {
                cost = char.ToLowerInvariant(s[i - 1]) == char.ToLowerInvariant(t_j) ? 0 : 1;
                // minimum of cell to the left+1, to the top+1, diagonally left and up +cost				
                d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
            }

            // copy current distance counts to 'previous row' distance counts
            _d = p;
            p = d;
            d = _d;
        }

        // our last action in the above loop was to switch d and p, so p now 
        // actually has the most recent cost counts
        return p[n];
    }
}