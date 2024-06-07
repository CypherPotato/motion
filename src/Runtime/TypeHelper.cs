using Motion.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Motion.Runtime;

#pragma warning disable IL2026

internal static class TypeHelper
{
    static Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
    static Dictionary<string, Type> typeLoadCache = null!;

    static void PreloadAssemblyCache()
    {
        for (int i = 0; i < currentAssemblies.Length; i++)
        {
            Type[] types = currentAssemblies[i].GetTypes();
            for (int j = 0; j < types.Length; j++)
            {
                Type t = types[j];
                if (t.IsNotPublic)
                {
                    continue;
                }
                typeLoadCache.TryAdd(t.FullName ?? t.Name, t);
            }
        }
    }

    public static Type? ResolveType(string name)
    {
        switch (name)
        {
            case "string": return typeof(string);

            case "short": return typeof(short);
            case "int": return typeof(int);
            case "long": return typeof(long);

            case "ushort": return typeof(ushort);
            case "uint": return typeof(uint);
            case "ulong": return typeof(ulong);

            case "float": return typeof(float);
            case "double": return typeof(double);
            case "decimal": return typeof(decimal);

            case "byte": return typeof(byte);
            case "sbyte": return typeof(sbyte);

            default:

                if (typeLoadCache is null)
                {
                    typeLoadCache = new Dictionary<string, Type>(AtomBase.SymbolComparer);
                    PreloadAssemblyCache();
                }

                if (typeLoadCache.TryGetValue(name, out Type? type))
                {
                    return type;
                }

                return null;
        }
    }

    public static Type? ResolveTypeByAtom(ref AtomBase atom)
    {
        if (atom.Keyword is not null)
        {
            Type? evaluatedType = ResolveType(atom.Keyword);
            if (evaluatedType is null)
            {
                throw new MotionException($"unresolved type '{atom.Keyword}'", atom.Location, null);
            }
            return evaluatedType;
        }
        else
        {
            switch (atom.Type)
            {
                case TokenType.String:
                case TokenType.Boolean:
                case TokenType.Number:
                case TokenType.Character:
                    return atom.Content!.GetType();
                default:
                    return null;
            }
        }
    }

    public static MethodInfo? FindMatchingMethodInfo(string name, MethodInfo[] methods, IList<Type?> typeHints, bool parameterLess)
    {
        Dictionary<float, MethodInfo> matched = new Dictionary<float, MethodInfo>();
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo m = methods[i];
            if (AtomBase.SymbolComparer.Compare(name, m.Name) != 0)
            {
                continue;
            }

            bool isMatched = true;
            float currentScore = 0;
            var mparams = m.GetParameters();

            if (typeHints.Count > 0)
            {
                if (typeHints.Count != mparams.Length)
                {
                    continue;
                }
                for (int j = 0; j < mparams.Length; j++)
                {
                    var mp = mparams[j];
                    var ap = typeHints[j];
                    if (ap is Type t)
                    {
                        if (t == typeof(object))
                        {
                            currentScore += 0.1f;
                        }
                        else if (t == mp.ParameterType)
                        {
                            currentScore += 10f;
                        }
                        else if (t.IsAssignableTo(mp.ParameterType))
                        {
                            float specScore = 1.0f;
                            Type? cursor = t.BaseType;
                            while (cursor?.BaseType is Type tparent)
                            {
                                specScore += 0.1f;
                                cursor = tparent.BaseType;
                            }
                        }
                        else
                        {
                            isMatched = false;
                        }
                    }
                }
                if (isMatched && !matched.TryAdd(currentScore, m))
                {
                    throw new MethodAccessException($"""
                                                     ambiguous method match between
                                                     - {name} {string.Join(" ", matched[currentScore].GetParameters().Select(s => s.ParameterType.Name))}
                                                     and
                                                     - {name} {string.Join(" ", m.GetParameters().Select(s => s.ParameterType.Name))}
                                                     """);
                }
            }
            else if (parameterLess && mparams.Length == 0)
            {
                return m;
            }
        }

        if (matched.Count == 0)
        {
            return null;
        }
        else
        {
            return matched.MaxBy(n => n.Key).Value;
        }
    }
}
