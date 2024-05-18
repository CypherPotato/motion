using Motion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCLI;

internal static class ServerMessenger
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

            if (Compiler.GetParenthesisIndex(code) <= 0)
            {
                buildingExpression.Clear();
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Post, Program.ServerEndpoint);
                    reqMsg.Content = new StringContent(code, Encoding.UTF8, "application/x-motion-code");

                    HttpResponseMessage resMsg = client.Send(reqMsg);
                    string? response = resMsg.Content.ReadAsStringAsync().Result;
                    if (resMsg.IsSuccessStatusCode)
                    {
                        Console.WriteLine(response);
                    }
                    else
                    {
                        if (Program.EnableColors) Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(response);
                        if (Program.EnableColors) Console.ResetColor();
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
