using Credfeto.Database.Source.Generation.Builders;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class ExtractColumns
{
    public static string? GenerateExtractColumnMapper(CodeBuilder source, string typeName)
    {
        return typeName switch
        {
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

    private static void EnsureColumnIsNotNull(CodeBuilder source)
    {
        using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
        {
            source.AppendLine("throw new DataException($\"Column {columName} is not nullable\");");
        }
    }

    private static void ReturnNullIfNull(CodeBuilder source)
    {
        using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
        {
            source.AppendLine("return null;");
        }
    }

    private static string ExtractInt8(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt8);

        using (source.StartBlock($"static sbyte {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (sbyte)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt8(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt8);

        using (source.StartBlock($"static sbyte? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (sbyte)value");
        }

        return methodName;
    }

    private static string ExtractUInt8(CodeBuilder source)
    {
        const string methodName = nameof(ExtractUInt8);

        using (source.StartBlock($"static byte {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (byte)value");
        }

        return methodName;
    }

    private static string ExtractNullableUInt8(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableUInt8);

        using (source.StartBlock($"static byte? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (byte)value");
        }

        return methodName;
    }

    private static string ExtractInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt16);

        using (source.StartBlock($"static short {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (short)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt16);

        using (source.StartBlock($"static short? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (short)value");
        }

        return methodName;
    }

    private static string ExtractUInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractUInt16);

        using (source.StartBlock($"static ushort {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (ushort)value");
        }

        return methodName;
    }

    private static string ExtractNullableUInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableUInt16);

        using (source.StartBlock($"static ushort? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (ushort)value");
        }

        return methodName;
    }

    private static string ExtractInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt32);

        using (source.StartBlock($"static int {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (int)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt32);

        using (source.StartBlock($"static int? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (int)value");
        }

        return methodName;
    }

    private static string ExtractUInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractUInt32);

        using (source.StartBlock($"static uint {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (uint)value");
        }

        return methodName;
    }

    private static string ExtractNullableUInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableUInt32);

        using (source.StartBlock($"static uint? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (uint)value");
        }

        return methodName;
    }

    private static string ExtractInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt64);

        using (source.StartBlock($"static long {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (long)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt64);

        using (source.StartBlock($"static long? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (long)value");
        }

        return methodName;
    }

    private static string ExtractUInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractUInt64);

        using (source.StartBlock($"static ulong {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (ulong)value");
        }

        return methodName;
    }

    private static string ExtractNullableUInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableUInt64);

        using (source.StartBlock($"static ulong? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (ulong)value");
        }

        return methodName;
    }

    private static string ExtractFloat(CodeBuilder source)
    {
        const string methodName = nameof(ExtractFloat);

        using (source.StartBlock($"static float {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (float)value");
        }

        return methodName;
    }

    private static string ExtractNullableFloat(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableFloat);

        using (source.StartBlock($"static float? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (float)value");
        }

        return methodName;
    }

    private static string ExtractDouble(CodeBuilder source)
    {
        const string methodName = nameof(ExtractDouble);

        using (source.StartBlock($"static double {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (double)value");
        }

        return methodName;
    }

    private static string ExtractNullableDouble(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableDouble);

        using (source.StartBlock($"static double? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (double)value");
        }

        return methodName;
    }

    private static string ExtractDecimal(CodeBuilder source)
    {
        const string methodName = nameof(ExtractDecimal);

        using (source.StartBlock($"static decimal {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (decimal)value");
        }

        return methodName;
    }

    private static string ExtractNullableDecimal(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableDecimal);

        using (source.StartBlock($"static decimal? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (decimal)value");
        }

        return methodName;
    }

    private static string ExtractString(CodeBuilder source)
    {
        const string methodName = nameof(ExtractString);

        using (source.StartBlock($"static string {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (string)value");
        }

        return methodName;
    }

    private static string ExtractNullableString(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableString);

        using (source.StartBlock($"static string? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (string)value");
        }

        return methodName;
    }

    private static string ExtractDateTime(CodeBuilder source)
    {
        const string methodName = nameof(ExtractString);

        using (source.StartBlock($"static DateTime {methodName}(object value, string columName)"))
        {
            EnsureColumnIsNotNull(source);

            source.AppendLine("return (DateTime)value");
        }

        return methodName;
    }

    private static string ExtractNullableDateTime(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableDateTime);

        using (source.StartBlock($"static DateTime? {methodName}(object value, string columName)"))
        {
            ReturnNullIfNull(source);

            source.AppendLine("return (DateTime)value");
        }

        return methodName;
    }
}