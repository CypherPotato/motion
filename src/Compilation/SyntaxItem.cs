using Motion.Parser;
using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Compilation;

/// <summary>
/// Represents an syntax tree of an Motion code Atom.
/// </summary>
public class SyntaxItem
{
    /// <summary>
    /// Gets the string representation of this syntax item.
    /// </summary>
    public string Contents { get; }

    /// <summary>
    /// Gets the <see cref="SyntaxItemType"/> which represents this syntax item.
    /// </summary>
    public SyntaxItemType Type { get; }

    /// <summary>
    /// Gets the line position of this syntax item.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column position of this syntax item.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the absolute index of this syntax item.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the length of this syntax item.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the item current expression depth.
    /// </summary>
    public int ExpressionDepth { get; }

    internal SyntaxItem(string contents, SyntaxItemType type, int depth, TextInterpreterSnapshot snapshot)
    {
        Contents = contents;
        Type = type;
        Line = snapshot.Line;
        Column = snapshot.Column;
        Position = snapshot.Index;
        Length = snapshot.Length;
        ExpressionDepth = depth;
    }
}

/// <summary>
/// Represents the type of the <see cref="SyntaxItem"/> object.
/// </summary>
public enum SyntaxItemType
{
    /// <summary>
    /// Represents the "NIL" word.
    /// </summary>
    NullWord,

    /// <summary>
    /// Represents an string literal.
    /// </summary>
    StringLiteral,

    /// <summary>
    /// Represents an raw-string literal.
    /// </summary>
    RawStringLiteral,

    /// <summary>
    /// Represents an character literal.
    /// </summary>
    CharacterLiteral,

    /// <summary>
    /// Represents an number literal.
    /// </summary>
    NumberLiteral,

    /// <summary>
    /// Represents an boolean word literal.
    /// </summary>
    BooleanLiteral,

    /// <summary>
    /// Represents the atom opening character.
    /// </summary>
    ExpressionStart,

    /// <summary>
    /// Represents the atom closing character.
    /// </summary>
    ExpressionEnd,

    /// <summary>
    /// Represents the array opening character.
    /// </summary>
    ArrayStart,

    /// <summary>
    /// Represents the array closing character.
    /// </summary>
    ArrayEnd,

    /// <summary>
    /// Represents an symbol.
    /// </summary>
    Symbol,

    /// <summary>
    /// Represents an CLR symbol.
    /// </summary>
    ClrSymbol,

    /// <summary>
    /// Represents an CLR type literal.
    /// </summary>
    ClrType,

    /// <summary>
    /// Represents an keyword.
    /// </summary>
    Keyword,

    /// <summary>
    /// Represents an operator.
    /// </summary>
    Operator,

    /// <summary>
    /// Represents an comment.
    /// </summary>
    Comment,

    /// <summary>
    /// Represents an unrecognized token.
    /// </summary>
    Unknown
}