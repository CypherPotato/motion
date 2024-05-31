using Motion.Runtime;
using PrettyPrompt.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MotionCLI;

public class AutoMenuFunctions
{
    public static FormattedString BuildVariableAutomenu(KeyValuePair<string, object?> information)
    {
        var b = new FormattedString();

        b += new FormattedString("variable ");
        b += new FormattedString(information.Key, Program.Theme.MenuVariable);
        b += new FormattedString("\n\n");
        b += new FormattedString("type: ");

        if (information.Value is null)
        {
            b += new FormattedString("<NIL>", Program.Theme.MenuHighlight);
        }
        else
        {
            Type t = information.Value.GetType();
            b += new FormattedString(t.Namespace + '.');
            b += new FormattedString(t.Name, Program.Theme.MenuTypeName);
        }

        b += new FormattedString("\n");

        return b;
    }

    public static FormattedString BuildConstantAutomenu(KeyValuePair<string, object?> information)
    {
        var b = new FormattedString();

        b += new FormattedString("constant ");
        b += new FormattedString(information.Key, Program.Theme.MenuConstant);
        b += new FormattedString("\n\n");
        b += new FormattedString("type: ");

        if (information.Value is null)
        {
            b += new FormattedString("<NIL>", Program.Theme.MenuHighlight);
        }
        else
        {
            Type t = information.Value.GetType();
            b += new FormattedString(t.Namespace + '.');
            b += new FormattedString(t.Name, Program.Theme.MenuTypeName);
        }

        b += new FormattedString("\n");

        return b;
    }

    public static FormattedString BuildUserFunctionAutomenu(KeyValuePair<string, MotionUserFunction> information)
    {
        var b = new FormattedString();

        b += new FormattedString("user function ");
        b += new FormattedString(information.Key, Program.Theme.MenuUserFunction);
        b += new FormattedString("\n");

        var parameters = information.Value.Arguments;
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            b += new FormattedString("-   :" + p, Program.Theme.Keyword);
            b += new FormattedString("\n");
        }

        b += new FormattedString("\n");
        b += new FormattedString(information.Value.Documentation);
        b += new FormattedString("\n");

        return b;
    }

    public static FormattedString BuildMethodAutomenu(KeyValuePair<string, Delegate> information)
    {
        var parameters = information.Value.Method.GetParameters();
        var b = new FormattedString();

        b += new FormattedString("method ");
        b += new FormattedString(information.Key, Program.Theme.MenuUserFunction);
        b += new FormattedString("\n");

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (i == 0 && p.Name == "self")
                continue;

            b += new FormattedString("-   :" + p.Name, Program.Theme.Keyword);
            b += new FormattedString(" ");

            if (p.IsOptional)
                b += new FormattedString("[");

            b += new FormattedString($"{p.ParameterType.Namespace}.");
            b += new FormattedString(p.ParameterType.Name, Program.Theme.MenuTypeName);

            if (p.IsOptional)
                b += new FormattedString("]");

            b += new FormattedString("\n");
        }

        b += new FormattedString("\n\n");

        b += new FormattedString("returns: ");
        Type t = information.Value.Method.ReturnType;
        b += new FormattedString(t.Namespace + '.');
        b += new FormattedString(t.Name, Program.Theme.MenuTypeName);

        return b;
    }
}
