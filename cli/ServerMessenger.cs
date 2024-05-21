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
using LightJson;
using Spectre.Console;
using Spectre.Console.Json;

namespace MotionCLI;

internal static class ServerMessenger
{
    public static async Task Init()
    {
        Console.WriteLine($"Motion Messaging Client [Cli. {Program.ClientVersionString}/Lang. {Motion.Compiler.MotionVersion}]");

        var promptString = "";
        var motionPromptCallback = new MotionPromptCallback();

        // connect to server
        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, Program.ServerEndpoint);
            requestMessage.Headers.TryAddWithoutValidation("Authorization", Program.ServerAuth);

            HttpResponseMessage resMsg = client.Send(requestMessage);
            if (resMsg.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JsonObject json = JsonValue.Deserialize(resMsg.Content.ReadAsStringAsync().Result).GetJsonObject();

                promptString = json["domain"].GetString() + "> ";

                foreach (JsonValue s in json["symbols"]["methods"].GetJsonArray())
                    motionPromptCallback.AutocompleteTerms.Add((s.GetString(), AnsiColor.Rgb(255, 233, 194), $"Remote method {s.GetString()}"));
                foreach (JsonValue s in json["symbols"]["user_functions"].GetJsonArray())
                    motionPromptCallback.AutocompleteTerms.Add((s.GetString(), AnsiColor.Rgb(242, 255, 204), $"Remote function {s.GetString()}"));
                foreach (JsonValue s in json["symbols"]["variables"].GetJsonArray())
                    motionPromptCallback.AutocompleteTerms.Add((s.GetString(), AnsiColor.Rgb(207, 247, 255), $"Remote variable {s.GetString()}"));
                foreach (JsonValue s in json["symbols"]["constants"].GetJsonArray())
                    motionPromptCallback.AutocompleteTerms.Add((s.GetString(), AnsiColor.Rgb(194, 218, 255), $"Remote constant {s.GetString()}"));
                foreach (JsonValue s in json["symbols"]["aliases"].GetJsonArray())
                    motionPromptCallback.AutocompleteTerms.Add((s.GetString(), AnsiColor.Rgb(255, 209, 245), $"Remote alias {s.GetString()}"));
            }
            else if (resMsg.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("Couldn't connect to the remote server: access danied for the provided authorization (forbidden)");
                return;
            }
            else if (resMsg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Couldn't connect to the remote server: authorization required (unauthorized)");
                return;
            }
            else
            {
                Console.WriteLine("Couldn't connect to the remote server: " + resMsg.ReasonPhrase);
                return;
            }
        }

        await using var prompt = new Prompt(
            persistentHistoryFilepath: "./history-file",
            callbacks: motionPromptCallback,
            configuration: new PromptConfiguration(
                prompt: new FormattedString(promptString),
                completionItemDescriptionPaneBackground: AnsiColor.Rgb(30, 30, 30),
                selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                selectedTextBackground: AnsiColor.Rgb(20, 61, 102)));

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

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Post, Program.ServerEndpoint);
                reqMsg.Headers.TryAddWithoutValidation("Authorization", Program.ServerAuth);
                reqMsg.Content = new StringContent(data, Encoding.UTF8, "application/x-motion-code");

                HttpResponseMessage resMsg = client.Send(reqMsg);
                string? dres = resMsg.Content.ReadAsStringAsync().Result;

                if (resMsg.Content.Headers.ContentType?.MediaType?.Contains("/json") == true)
                {
                    AnsiConsole.Write(new JsonText(dres));
                }
                else
                {
                    Console.WriteLine(dres.ReplaceLineEndings());
                }

                Console.WriteLine();
            }
        }
    }
}
