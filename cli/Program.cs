using Motion;
using Motion.Runtime;
using System.Diagnostics;
using System.Text;

namespace MotionCLI;

internal class Program
{
    public static bool EnableColors = true;
    public static bool Verbose = true;
    public static string? ServerEndpoint = null;
    public static string? ServerUsername = null;
    public static string? ServerPassword = null;

    static void Main(string[] args)
    {
        var cmdParser = new CommandLine.CommandLineParser(args);

        EnableColors = !cmdParser.IsDefined("no-colors");
        Verbose = cmdParser.IsDefined("verbose", 'v');
        ServerEndpoint = cmdParser.GetValue("endpoit", 'e');

        if (ServerEndpoint is null)
        {
            Interactive.Init();
        }
        else
        {
            ServerMessenger.Init();
        }

        if (EnableColors) Console.ResetColor();
    }
}