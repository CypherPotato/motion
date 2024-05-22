using LightJson;
using LightJson.Converters;
using PrettyPrompt.Highlighting;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCLI;

public class Theme
{
    public ConsoleFormat WordLiteral { get; set; } = new ConsoleFormat(AnsiColor.Rgb(92, 195, 250));
    public ConsoleFormat StringLiteral { get; set; } = new ConsoleFormat(AnsiColor.Rgb(255, 102, 102));
    public ConsoleFormat NumberLiteral { get; set; } = new ConsoleFormat(AnsiColor.Rgb(185, 250, 225));
    public ConsoleFormat FunctionName { get; set; } = new ConsoleFormat(AnsiColor.Rgb(255, 233, 194));
    public ConsoleFormat Symbol { get; set; } = new ConsoleFormat(AnsiColor.Rgb(216, 235, 240));
    public ConsoleFormat Keyword { get; set; } = new ConsoleFormat(AnsiColor.Rgb(237, 207, 255));
    public ConsoleFormat Comment { get; set; } = new ConsoleFormat(AnsiColor.Rgb(106, 173, 127));

    public ConsoleFormat MenuVariable { get; set; } = new ConsoleFormat(AnsiColor.Rgb(207, 247, 255));
    public ConsoleFormat MenuConstant { get; set; } = new ConsoleFormat(AnsiColor.Rgb(194, 218, 255));
    public ConsoleFormat MenuUserFunction { get; set; } = new ConsoleFormat(AnsiColor.Rgb(255, 233, 194));
    public ConsoleFormat MenuMethod { get; set; } = new ConsoleFormat(AnsiColor.Rgb(242, 255, 204));
    public ConsoleFormat MenuAlias { get; set; } = new ConsoleFormat(AnsiColor.Rgb(255, 209, 245));
    public ConsoleFormat MenuHighlight { get; set; } = new ConsoleFormat(AnsiColor.Rgb(255, 102, 102));
    public ConsoleFormat MenuTypeName { get; set; } = new ConsoleFormat(AnsiColor.Rgb(185, 250, 225));
}

class ThemeConverter : JsonConverter
{
    public override bool CanSerialize(Type type)
    {
        return type == typeof(Theme);
    }

    public override object Deserialize(JsonValue value, Type requestedType)
    {
        return new Theme()
        {
            Comment = value["comment"].Get<ConsoleFormat>(),
            StringLiteral = value["stringLiteral"].Get<ConsoleFormat>(),
            FunctionName = value["functionName"].Get<ConsoleFormat>(),
            Keyword = value["keyword"].Get<ConsoleFormat>(),
            NumberLiteral = value["numberLiteral"].Get<ConsoleFormat>(),
            WordLiteral = value["wordLiteral"].Get<ConsoleFormat>(),
            Symbol = value["symbol"].Get<ConsoleFormat>(),
        };
    }

    public override JsonValue Serialize(object value)
    {
        Theme t = (Theme)value;
        return new JsonObject()
        {
            ["comment"] = JsonValue.Serialize(t.Comment),
            ["stringLiteral"] = JsonValue.Serialize(t.StringLiteral),
            ["functionName"] = JsonValue.Serialize(t.FunctionName),
            ["keyword"] = JsonValue.Serialize(t.Keyword),
            ["numberLiteral"] = JsonValue.Serialize(t.NumberLiteral),
            ["wordLiteral"] = JsonValue.Serialize(t.WordLiteral),
            ["symbol"] = JsonValue.Serialize(t.Symbol),
        };
    }
}

class ConsoleFormatConverter : JsonConverter
{
    public override bool CanSerialize(Type type)
    {
        return type == typeof(ConsoleFormat);
    }

    public override object Deserialize(JsonValue value, Type requestedType)
    {
        string hex = value["color"].GetString();
        byte red = (byte)Convert.ToInt32(hex[0..1], 16);
        byte green = (byte)Convert.ToInt32(hex[2..3], 16);
        byte blue = (byte)Convert.ToInt32(hex[4..5], 16);

        bool bold = value["bold"].MaybeNull()?.GetBoolean() ?? false;

        return new ConsoleFormat(AnsiColor.Rgb(red, green, blue), Bold: bold);
    }

    public override JsonValue Serialize(object value)
    {
        ConsoleFormat fmt = (ConsoleFormat)value;
        var obj = new JsonObject()
        {
            ["color"] = fmt.Foreground!.Value.ToString()
        };
        return obj;
    }
}