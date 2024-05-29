using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion;

/// <summary>
/// Represents an export of an <see cref="Enum"/> type.
/// </summary>
public sealed class EnumExport
{
    /// <summary>
    /// Gets the exporting <see cref="Enum"/>.
    /// </summary>
    public Type ExportType { get; init; }

    /// <summary>
    /// Gets the alias text of the exporting enum members.
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Creates an new instance of the <see cref="EnumExport"/> class with specified
    /// parameters.
    /// </summary>
    /// <param name="exportType">The enum type which members will be exported.</param>
    /// <param name="alias">Optional. Defines the alias prefix for the exporting enum. If null, the enum type name will be used.</param>
    public EnumExport(Type exportType, string? alias)
    {
        if (!exportType.IsEnum) throw new ArgumentException("The specified type ins't an Enum type.");
        this.ExportType = exportType;
        this.Alias = alias;
    }

    /// <summary>
    /// Creates an new instance of the <see cref="EnumExport"/> class with specified
    /// parameters.
    /// </summary>
    /// <typeparam name="TEnum">The enum type which members will be exported.</typeparam>
    /// <param name="alias">Optional. Defines the alias prefix for the exporting enum. If null, the enum type name will be used.</param>
    /// <returns>An new <see cref="EnumExport"/> instance.</returns>
    public static EnumExport Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string? alias = null) where TEnum : Enum
    {
        return new EnumExport(typeof(TEnum), alias);
    }
}
