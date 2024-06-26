﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary.Common;

internal class ComFunctions : IMotionLibrary
{
    public string? Namespace => null;

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("defun", (Atom self) => FunctionDefinition(self, true));
        context.Methods.Add("defmacro", (Atom self) => FunctionDefinition(self, false));
        context.Methods.Add("import", Import);

        context.Methods.Add("get-trace", GetTrace);
        context.Methods.Add("help", GetCommands);
    }

    string GetCommands(Atom self)
    {
        self.EnsureExactItemCount(1, 2);

        bool IsSimilar(string key, string? term) =>
            term is null ? true :
            (key.Contains(term, StringComparison.CurrentCultureIgnoreCase) || StdString.ComputeLevenshteinDistance(key.ToLower(), term.ToLower()) <= 2);

        string? similar = null;
        if (self.ItemCount == 2)
        {
            similar = self.GetAtom(1).GetString();
        }

        List<string> methods = new List<string>();
        List<string> userMethods = new List<string>();
        List<string> constants = new List<string>();
        List<string> variables = new List<string>();

        foreach (string key in self.Context.Methods.Keys) if (IsSimilar(key, similar)) methods.Add(key);
        foreach (string key in self.Context.UserFunctions.Keys) if (IsSimilar(key, similar)) userMethods.Add(key);
        foreach (string key in self.Context.Constants.Keys) if (IsSimilar(key, similar)) constants.Add(key);
        foreach (string key in self.Context.Variables.Keys) if (IsSimilar(key, similar)) variables.Add(key);

        StringBuilder sb = new StringBuilder();
        if (methods.Any())
        {
            methods.Sort();
            sb.Append("Methods:\n- ");
            sb.AppendLine(string.Join("\n- ", methods));
            sb.AppendLine();
        }
        if (userMethods.Any())
        {
            userMethods.Sort();
            sb.Append("User methods:\n- ");
            sb.AppendLine(string.Join("\n- ", userMethods));
            sb.AppendLine();
        }
        if (constants.Any())
        {
            constants.Sort();
            sb.Append("Constants:\n- ");
            sb.AppendLine(string.Join("\n- ", constants));
            sb.AppendLine();
        }
        if (variables.Any())
        {
            variables.Sort();
            sb.Append("Variables:\n- ");
            sb.AppendLine(string.Join("\n- ", variables));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    string GetTrace(Atom self)
    {
        return self.Context.traceWriter.ToString();
    }

    void Import(Atom self, Symbol name)
    {
        self.Context.Global.UsingStatements.Add(name.Contents);
    }

    string FunctionDefinition(Atom self, bool isFunction)
    {
        self.EnsureMinimumItemCount(4);
        string name = self.GetAtom(1).GetSymbol();
        string[] args;
        string? documentation;
        Atom body;

        if (self.ItemCount == 5)
        {
            // has docs
            args = self.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
            documentation = self.GetAtom(3).GetString();
            body = self.GetAtom(4);
        }
        else if (self.ItemCount == 4)
        {
            args = self.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
            documentation = null;
            body = self.GetAtom(3);
        }
        else
        {
            self.EnsureExactItemCount(3, 4);
            return "";
        }

        MotionUserFunction func = new MotionUserFunction(args, documentation, body._ref, isFunction);
        self.Context.GetScope(ExecutionContextScope.Global).UserFunctions.Add(name, func);

        return name.ToUpper();
    }
}
