using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Motion.Runtime.StandardLibrary;

class StdCommon : IMotionLibrary
{
    public string? Namespace => null;

    public static void ApplyOpMethods(ExecutionContext context)
    {
        context.Methods.Add("+", (atom) =>
        {
            var firstItem = atom.GetAtom(1).GetObject();

            if (firstItem is string s)
            {
                StringBuilder result = new StringBuilder(s);

                for (int i = 2; i < atom.ItemCount; i++)
                {
                    result.Append(atom.GetAtom(i).Nullable()?.GetObject());
                }

                return result.ToString();
            }
            else
            {
                double carry = Convert.ToDouble(firstItem);
                for (int i = 2; i < atom.ItemCount; i++)
                {
                    carry += atom.GetAtom(i).GetDouble();
                }
                return carry;
            }
        });
        context.Methods.Add("-", (atom) =>
        {
            double carry = atom.GetAtom(1).GetDouble();
            for (int i = 2; i < atom.ItemCount; i++)
            {
                carry -= atom.GetAtom(i).GetDouble();
            }
            return carry;
        });
        context.Methods.Add("/", (atom) =>
        {
            double carry = atom.GetAtom(1).GetDouble();
            for (int i = 2; i < atom.ItemCount; i++)
            {
                carry /= atom.GetAtom(i).GetDouble();
            }
            return carry;
        });
        context.Methods.Add("*", (atom) =>
        {
            double carry = atom.GetAtom(1).GetDouble();
            for (int i = 2; i < atom.ItemCount; i++)
            {
                carry *= atom.GetAtom(i).GetDouble();
            }
            return carry;
        });
    }

    public static void ApplyBooleanOperators(ExecutionContext context)
    {
        context.Methods.Add("=", (atom) =>
        {
            object first = atom.GetAtom(1).GetObject();

            if (first is Double d)
            {
                return first.Equals(atom.GetAtom(2).Nullable()?.GetDouble());
            }
            else if (first is Int32 i)
            {
                return first.Equals(atom.GetAtom(2).Nullable()?.GetInt32());
            }
            else
            {
                return first.Equals(atom.GetAtom(2).Nullable()?.GetObject());
            }
        });
        context.Methods.Add("/=", (atom) =>
        {
            return !atom.GetAtom(1).GetObject()?.Equals(atom.GetAtom(2).GetObject());
        });
        context.Methods.Add("and", (atom) =>
        {
            bool state = atom.GetAtom(1).GetBoolean();

            for (int i = 2; i < atom.ItemCount; i++)
            {
                state = state && atom.GetAtom(i).GetBoolean();
            }

            return state;
        });
        context.Methods.Add("or", (atom) =>
        {
            bool state = atom.GetAtom(1).GetBoolean();

            for (int i = 2; i < atom.ItemCount; i++)
            {
                state = state || atom.GetAtom(i).GetBoolean();
            }

            return state;
        });
        context.Methods.Add("xor", (atom) =>
        {
            bool state = atom.GetAtom(1).GetBoolean();

            for (int i = 2; i < atom.ItemCount; i++)
            {
                state = state ^ atom.GetAtom(i).GetBoolean();
            }

            return state;
        });
        context.Methods.Add("not", (atom) =>
        {
            return !atom.GetAtom(1).GetBoolean();
        });
        context.Methods.Add(">", (atom) =>
        {
            return atom.GetAtom(1).GetDouble() > atom.GetAtom(2).GetDouble();
        });
        context.Methods.Add(">=", (atom) =>
        {
            return atom.GetAtom(1).GetDouble() >= atom.GetAtom(2).GetDouble();
        });
        context.Methods.Add("<", (atom) =>
        {
            return atom.GetAtom(1).GetDouble() < atom.GetAtom(2).GetDouble();
        });
        context.Methods.Add("<=", (atom) =>
        {
            return atom.GetAtom(1).GetDouble() <= atom.GetAtom(2).GetDouble();
        });
        context.Methods.Add("zerop", (atom) =>
        {
            return atom.GetAtom(1).GetDouble() == 0;
        });
    }

    public static void ApplyCondBlocks(ExecutionContext context)
    {
        context.Methods.Add("if", (atom) =>
        {
            if (atom.ItemCount == 3)
            {
                // if exp then
                bool? cond = atom.GetAtom(1).Nullable()?.GetBoolean();
                if (cond.HasValue && cond.Value == true)
                {
                    return atom.GetAtom(2).Nullable()?.GetObject();
                }

                return null;
            }
            else if (atom.ItemCount == 4)
            {
                // if exp then
                bool? cond = atom.GetAtom(1).Nullable()?.GetBoolean();
                if (cond.HasValue && cond.Value == true)
                {
                    return atom.GetAtom(2).Nullable()?.GetObject();
                }
                else
                {
                    return atom.GetAtom(3).Nullable()?.GetObject();
                }
            }
            else
            {
                throw new Exception($"expected two or three parameters. got {atom.ItemCount}.");
            }
        });
        context.Methods.Add("match", (atom) =>
        {
            object value = atom.GetAtom(1).GetObject();

            for (int i = 2; i < atom.ItemCount; i++)
            {
                Atom condSubAtom = atom.GetAtom(i);
                condSubAtom.EnsureExactItemCount(2);

                object? condValue = condSubAtom.GetAtom(0).Nullable()?.GetObject();

                if (value.Equals(condValue))
                {
                    return condSubAtom.GetAtom(1).Nullable()?.GetObject();
                }
            }

            return null;
        });
    }

    public static void ApplyKeyFunctions(ExecutionContext context)
    {
        context.Methods.Add("increment", (atom) =>
        {
            string varname = atom.GetAtom(1).GetSymbol();
            double increment = atom.GetAtom(2).GetDouble();

            double num = Convert.ToDouble(atom.Context.GetScope(ExecutionContextScope.Function).Variables.Get(varname));
            num += increment;
            atom.Context.GetScope(ExecutionContextScope.Function).Variables.Set(varname, num);

            return num;
        });
        context.Methods.Add("type-of", (atom) =>
        {
            object b = atom.GetAtom(1).GetObject();
            return b.GetType();
        });
        context.Methods.Add("exit", (atom) =>
        {
            object? value = atom.GetAtom(1).Nullable()?.GetObject();
            throw new MotionExitException(value);
        });
        context.Methods.Add("coalesce", (atom) =>
        {
            for (int i = 1; i < atom.ItemCount; i++)
            {
                object? data = atom.GetAtom(i).Nullable()?.GetObject();
                if (data is not null)
                {
                    return data;
                }
            }
            return null;
        });
    }

    public static void ApplyVarBlocks(ExecutionContext context)
    {
        context.Methods.Add("defalias", (atom) =>
        {
            string aliasName = atom.GetAtom(1).GetSymbol();
            string aliasValue = atom.GetAtom(2).GetSymbol();

            atom.Context.GetScope(ExecutionContextScope.Function).Aliases.Add(aliasName, aliasValue);
            return true;
        });
        context.Methods.Add("defvar", (atom) =>
        {
            string varname = atom.GetAtom(1).GetSymbol();
            object? value = atom.GetAtom(2).Nullable()?.GetObject();

            atom.Context.GetScope(ExecutionContextScope.Function).Variables.Add(varname, value);
            return value;
        });
        context.Methods.Add("defconstant", (atom) =>
        {
            string varname = atom.GetAtom(1).GetSymbol();
            object? value = atom.GetAtom(2).Nullable()?.GetObject();

            atom.Context.GetScope(ExecutionContextScope.Function).Constants.Add(varname, value);
            return value;
        });
        context.Methods.Add("set", (atom) =>
        {
            string varname = atom.GetAtom(1).GetSymbol();
            object? value = atom.GetAtom(2).Nullable()?.GetObject();

            var variables = atom.Context.GetScope(ExecutionContextScope.Function).Variables;
            if (!variables.Contains(varname))
                throw new Exception($"The variable {varname} is not defined.");

            variables.Set(varname, value);

            return value;
        });
        context.Methods.Add("let", (atom) =>
        {
            atom.EnsureExactItemCount(3);

            var varBlock = atom.GetAtom(1);
            var expBlock = atom.GetAtom(2);

            var varBlockAtoms = varBlock.GetAtoms();

            foreach (Atom varBlockAtom in varBlockAtoms)
            {
                var key = varBlockAtom.GetAtom(0).GetSymbol();
                var value = varBlockAtom.GetAtom(1).Nullable()?.GetObject();

                atom.Context.Variables.Set(key, value);
            }

            return expBlock.Nullable()?.GetObject();
        });
        context.Methods.Add("with", (atom) =>
        {
            atom.EnsureExactItemCount(4);

            var valueAtom = atom.GetAtom(1).Nullable()?.GetObject();
            var symbolAtom = atom.GetAtom(2).GetSymbol();

            atom.Context.Variables.Set(symbolAtom, valueAtom);
            var result = atom.GetAtom(3).Nullable()?.GetObject();

            if (valueAtom is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return result;
        });
        context.Methods.Add("use", (atom) =>
        {
            string[] usings = atom.GetAtoms()
                .Skip(1)
                .Select(a => a.GetSymbol())
                .ToArray();

            foreach (string s in usings)
                atom.Context.GetScope(ExecutionContextScope.Function).UsingStatements.Add(s);
            return true;
        });
    }

    public static void ApplyArrayBlocks(ExecutionContext context)
    {
        context.Methods.Add("make-array", (atom) =>
        {
            object?[] arr = new object?[atom.ItemCount - 1];
            for (int i = 1; i < atom.ItemCount; i++)
            {
                arr[i - 1] = atom.GetAtom(i).Nullable()?.GetObject();
            }

            return arr;
        });
        context.Methods.Add("dotimes", (atom) =>
        {
            atom.EnsureExactItemCount(3);

            ArrayList result = new ArrayList();
            var iteratorExp = atom.GetAtom(1);
            var iteratorVar = iteratorExp.GetAtom(0).GetSymbol();
            var iteratorMax = iteratorExp.GetAtom(1).GetInt32();

            var iteratorBody = atom.GetAtom(2);

            for (int i = 0; i < iteratorMax; i++)
            {
                atom.Context.SetVariable(iteratorVar, i);
                result.Add(iteratorBody.Nullable()?.GetObject());
            }

            return result;
        });
        context.Methods.Add("map", (atom) =>
        {
            atom.EnsureExactItemCount(3);

            ArrayList result = new ArrayList();
            var iteratorExp = atom.GetAtom(1);
            var iteratorArr = (IEnumerable)iteratorExp.GetAtom(0).GetObject();
            var iteratorVar = iteratorExp.GetAtom(1).GetSymbol();

            var iteratorBody = atom.GetAtom(2);

            foreach (var item in iteratorArr)
            {
                atom.Context.SetVariable(iteratorVar, item);
                result.Add(iteratorBody.Nullable()?.GetObject());
            }

            return result;
        });
        context.Methods.Add("while", (atom) =>
        {
            atom.EnsureExactItemCount(3);

            ArrayList result = new ArrayList();

            var conditionAtom = atom.GetAtom(1);
            var statementAtom = atom.GetAtom(2);

            while (conditionAtom.GetBoolean() == true)
            {
                result.Add(statementAtom.Nullable()?.GetObject());
            }

            return result;
        });
        context.Methods.Add("array-count", (atom) =>
        {
            atom.EnsureExactItemCount(2);

            var iteratorArr = (IList)atom.GetAtom(1).GetObject();
            return iteratorArr.Count;
        });
        context.Methods.Add("aref", (atom) =>
        {
            IList arr = (IList)atom.GetAtom(1).GetObject();
            int index = atom.GetAtom(2).GetInt32();

            return arr[index];
        });
    }

    public static void ApplyFunctionsBlocks(ExecutionContext context)
    {
        context.Methods.Add("defun", (atom) =>
        {
            string name = atom.GetAtom(1).GetSymbol();
            string[] args;
            string? documentation;
            Atom body;

            if (atom.ItemCount == 5)
            {
                // has docs
                args = atom.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
                documentation = atom.GetAtom(3).GetString();
                body = atom.GetAtom(4);
            }
            else if (atom.ItemCount == 4)
            {
                args = atom.GetAtom(2).GetAtoms().Select(a => a.GetSymbol()).ToArray();
                documentation = null;
                body = atom.GetAtom(3);
            }
            else
            {
                throw new InvalidOperationException($"defun requires at least 3 parameters. got {atom.ItemCount}.");
            }

            MotionUserFunction func = new MotionUserFunction(args, documentation, body._ref);
            context.GetScope(ExecutionContextScope.Global).UserFunctions.Add(name, func);

            return null;
        });
    }

    public void ApplyMembers(ExecutionContext context)
    {
        context.Constants.Add("motion-version", Compiler.MotionVersion);
        ApplyOpMethods(context);
        ApplyBooleanOperators(context);
        ApplyCondBlocks(context);
        ApplyVarBlocks(context);
        ApplyArrayBlocks(context);
        ApplyFunctionsBlocks(context);
        ApplyKeyFunctions(context);
    }
}
