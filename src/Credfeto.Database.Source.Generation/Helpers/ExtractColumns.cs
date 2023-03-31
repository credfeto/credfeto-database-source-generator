using System;
using Credfeto.Database.Source.Generation.Builders;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class ExtractColumns
{
    public static string? GenerateExtractColumnMapper(CodeBuilder source, string typeName)
    {
        return typeName switch
        {
            "bool" => ExtractBool(source: source),
            "bool?" => ExtractNullableBool(source: source),
            "byte" => ExtractUInt8(source: source),
            "byte?" => ExtractNullableUInt8(source: source),
            "sbyte" => ExtractInt8(source: source),
            "sbyte?" => ExtractNullableInt8(source: source),
            "short" => ExtractInt16(source: source),
            "short?" => ExtractNullableInt16(source: source),
            "ushort" => ExtractUInt16(source: source),
            "ushort?" => ExtractNullableUInt16(source: source),
            "int" => ExtractInt32(source: source),
            "int?" => ExtractNullableInt32(source: source),
            "uint" => ExtractUInt32(source: source),
            "uint?" => ExtractNullableUInt32(source: source),
            "long" => ExtractInt64(source: source),
            "long?" => ExtractNullableInt64(source: source),
            "ulong" => ExtractUInt64(source: source),
            "ulong?" => ExtractNullableUInt64(source: source),
            "float" => ExtractFloat(source: source),
            "float?" => ExtractNullableFloat(source: source),
            "double" => ExtractDouble(source: source),
            "double?" => ExtractNullableDouble(source: source),
            "decimal" => ExtractDecimal(source: source),
            "decimal?" => ExtractNullableDecimal(source: source),
            "string" => ExtractString(source: source),
            "string?" => ExtractNullableString(source: source),
            "System.DateTime" => ExtractDateTime(source: source),
            "System.DateTime?" => ExtractNullableDateTime(source: source),
            _ => null
        };
    }

    public static string? GenerateReturn(string typeName, string variable)
    {
        return typeName switch
        {
            "bool" => ReturnBool(variable: variable),
            "byte" => ReturnUInt8(variable: variable),
            "sbyte" => ReturnInt8(variable: variable),
            "short" => ReturnInt16(variable: variable),
            "ushort" => ReturnUInt16(variable: variable),
            "int" => ReturnInt32(variable: variable),
            "uint" => ReturnUInt32(variable: variable),
            "long" => ReturnInt64(variable: variable),
            "ulong" => ReturnUInt64(variable: variable),
            "float" => ReturnFloat(variable: variable),
            "double" => ReturnDouble(variable: variable),
            "decimal" => ReturnDecimal(variable: variable),
            "string" => ReturnString(variable: variable),
            "System.DateTime" => ReturnDateTime(variable: variable),
            _ => null
        };
    }

    private static string ExtractCommon(CodeBuilder source, string typeName, string methodName, bool isNullable, Func<string, string> getReturnStatement)
    {
        const string valueParameter = "value";
        string nullableMarker = isNullable
            ? "?"
            : string.Empty;

        using (source.StartBlock($"static {typeName}{nullableMarker} {methodName}(object {valueParameter}, string columName)"))
        {
            using (source.StartBlock("if (value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine(isNullable
                                      ? "return null;"
                                      : "throw new DataException($\"Column {columName} is not nullable\");");
            }

            source.AppendBlankLine()
                  .AppendLine(getReturnStatement(valueParameter));
        }

        return methodName;
    }

    private static string ReturnInt8(string variable)
    {
        return $"return Convert.ToSByte({variable});";
    }

    private static string ReturnBool(string variable)
    {
        return $"return Convert.ToBoolean({variable});";
    }

    private static string ReturnUInt8(string variable)
    {
        return $"return Convert.ToByte({variable});";
    }

    private static string ReturnInt16(string variable)
    {
        return $"return Convert.ToInt16({variable});";
    }

    private static string ReturnUInt16(string variable)
    {
        return $"return Convert.ToUInt16({variable});";
    }

    private static string ReturnInt32(string variable)
    {
        return $"return Convert.ToInt32({variable});";
    }

    private static string ReturnUInt32(string variable)
    {
        return $"return Convert.ToUInt32({variable});";
    }

    private static string ReturnInt64(string variable)
    {
        return $"return Convert.ToInt64({variable});";
    }

    private static string ReturnUInt64(string variable)
    {
        return $"return Convert.ToUInt64({variable});";
    }

    private static string ReturnDateTime(string variable)
    {
        return $"return Convert.ToDateTime({variable});";
    }

    private static string ReturnString(string variable)
    {
        return $"return (string){variable};";
    }

    private static string ReturnDecimal(string variable)
    {
        return $"return Convert.Decimal({variable});";
    }

    private static string ReturnDouble(string variable)
    {
        return $"return Convert.Double({variable});";
    }

    private static string ReturnFloat(string variable)
    {
        return $"return Convert.Double({variable});";
    }

    private static string ExtractBool(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "bool", nameof(ExtractBool), isNullable: false, getReturnStatement: ReturnBool);
    }

    private static string ExtractNullableBool(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "bool", nameof(ExtractNullableBool), isNullable: true, getReturnStatement: ReturnBool);
    }

    private static string ExtractInt8(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "sbyte", nameof(ExtractInt8), isNullable: false, getReturnStatement: ReturnInt8);
    }

    private static string ExtractNullableInt8(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "sbyte", nameof(ExtractNullableInt8), isNullable: true, getReturnStatement: ReturnInt8);
    }

    private static string ExtractUInt8(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "byte", nameof(ExtractUInt8), isNullable: false, getReturnStatement: ReturnUInt8);
    }

    private static string ExtractNullableUInt8(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "byte", nameof(ExtractNullableUInt8), isNullable: true, getReturnStatement: ReturnUInt8);
    }

    private static string ExtractInt16(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "short", nameof(ExtractInt16), isNullable: false, getReturnStatement: ReturnInt16);
    }

    private static string ExtractNullableInt16(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "short", nameof(ExtractNullableInt16), isNullable: true, getReturnStatement: ReturnInt16);
    }

    private static string ExtractUInt16(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "ushort", nameof(ExtractUInt16), isNullable: false, getReturnStatement: ReturnUInt16);
    }

    private static string ExtractNullableUInt16(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "ushort", nameof(ExtractNullableUInt16), isNullable: true, getReturnStatement: ReturnUInt16);
    }

    private static string ExtractInt32(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "int", nameof(ExtractInt32), isNullable: false, getReturnStatement: ReturnInt32);
    }

    private static string ExtractNullableInt32(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "int", nameof(ExtractNullableInt32), isNullable: true, getReturnStatement: ReturnInt32);
    }

    private static string ExtractUInt32(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "uint", nameof(ExtractUInt32), isNullable: false, getReturnStatement: ReturnUInt32);
    }

    private static string ExtractNullableUInt32(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "uint", nameof(ExtractNullableUInt32), isNullable: true, getReturnStatement: ReturnUInt32);
    }

    private static string ExtractInt64(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "long", nameof(ExtractInt64), isNullable: false, getReturnStatement: ReturnInt64);
    }

    private static string ExtractNullableInt64(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "long", nameof(ExtractInt64), isNullable: true, getReturnStatement: ReturnInt64);
    }

    private static string ExtractUInt64(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "ulong", nameof(ExtractUInt64), isNullable: false, getReturnStatement: ReturnUInt64);
    }

    private static string ExtractNullableUInt64(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "ulong", nameof(ExtractUInt64), isNullable: true, getReturnStatement: ReturnUInt64);
    }

    private static string ExtractFloat(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "float", nameof(ExtractFloat), isNullable: false, getReturnStatement: ReturnFloat);
    }

    private static string ExtractNullableFloat(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "float", nameof(ExtractNullableFloat), isNullable: true, getReturnStatement: ReturnFloat);
    }

    private static string ExtractDouble(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "double", nameof(ExtractDouble), isNullable: false, getReturnStatement: ReturnDouble);
    }

    private static string ExtractNullableDouble(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "double", nameof(ExtractNullableDouble), isNullable: true, getReturnStatement: ReturnDouble);
    }

    private static string ExtractDecimal(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "decimal", nameof(ExtractDecimal), isNullable: false, getReturnStatement: ReturnDecimal);
    }

    private static string ExtractNullableDecimal(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "decimal", nameof(ExtractNullableDecimal), isNullable: true, getReturnStatement: ReturnDecimal);
    }

    private static string ExtractString(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "string", nameof(ExtractString), isNullable: false, getReturnStatement: ReturnString);
    }

    private static string ExtractNullableString(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "string", nameof(ExtractNullableString), isNullable: true, getReturnStatement: ReturnString);
    }

    private static string ExtractDateTime(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "DateTime", nameof(ExtractDateTime), isNullable: false, getReturnStatement: ReturnDateTime);
    }

    private static string ExtractNullableDateTime(CodeBuilder source)
    {
        return ExtractCommon(source: source, typeName: "DateTime", nameof(ExtractNullableDateTime), isNullable: true, getReturnStatement: ReturnDateTime);
    }
}