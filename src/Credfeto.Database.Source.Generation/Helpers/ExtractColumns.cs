using Credfeto.Database.Source.Generation.Builders;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class ExtractColumns
{
    public static string? GenerateExtractColumnMapper(CodeBuilder source, string typeName)
    {
        return typeName switch
        {
            "short" => ExtractInt16(source: source),
            "short?" => ExtractNullableInt16(source: source),
            "int" => ExtractInt32(source: source),
            "int?" => ExtractNullableInt32(source: source),
            "long" => ExtractInt64(source: source),
            "long?" => ExtractNullableInt64(source: source),
            "string" => ExtractString(source: source),
            "string?" => ExtractNullableString(source: source),
            _ => null
        };
    }

    private static string ExtractInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt16);

        using (source.StartBlock($"static short {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("throw new DataException($\"Column {columName} is not nullable\");");
            }

            source.AppendLine("return (short)value");
        }

        return methodName;
    }

    private static string ExtractInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt32);

        using (source.StartBlock($"static int {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("throw new DataException($\"Column {columName} is not nullable\");");
            }

            source.AppendLine("return (int)value");
        }

        return methodName;
    }

    private static string ExtractInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractInt64);

        using (source.StartBlock($"static int {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("throw new DataException($\"Column {columName} is not nullable\");");
            }

            source.AppendLine("return (int)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt16(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt16);

        using (source.StartBlock($"static short? {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("return null;");
            }

            source.AppendLine("return (short)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt32(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt32);

        using (source.StartBlock($"static int? {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("return null;");
            }

            source.AppendLine("return (int)value");
        }

        return methodName;
    }

    private static string ExtractNullableInt64(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableInt64);

        using (source.StartBlock($"static long? {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("return null;");
            }

            source.AppendLine("return (long)value");
        }

        return methodName;
    }

    private static string ExtractString(CodeBuilder source)
    {
        const string methodName = nameof(ExtractString);

        using (source.StartBlock($"static string {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("throw new DataException($\"Column {columName} is not nullable\");");
            }

            source.AppendLine("return (string)value");
        }

        return methodName;
    }

    private static string ExtractNullableString(CodeBuilder source)
    {
        const string methodName = nameof(ExtractNullableString);

        using (source.StartBlock($"static string? {methodName}(object value, string columName)"))
        {
            using (source.StartBlock("if(value == null || Convert.IsDBNull(value))"))
            {
                source.AppendLine("return null;");
            }

            source.AppendLine("return (string)value");
        }

        return methodName;
    }
}