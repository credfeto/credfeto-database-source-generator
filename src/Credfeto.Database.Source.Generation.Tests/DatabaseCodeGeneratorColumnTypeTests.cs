using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorColumnTypeTests : TestBase
{
    private static string BuildTableFunctionSource(string columnType)
    {
        return """
            using System;
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                public record TestRow(COLUMN_TYPE Value);

                [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """.Replace(oldValue: "COLUMN_TYPE", newValue: columnType, comparisonType: StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("bool", "ExtractBool")]
    [InlineData("bool?", "ExtractNullableBool")]
    [InlineData("byte", "ExtractUInt8")]
    [InlineData("byte?", "ExtractNullableUInt8")]
    [InlineData("sbyte", "ExtractInt8")]
    [InlineData("sbyte?", "ExtractNullableInt8")]
    [InlineData("short", "ExtractInt16")]
    [InlineData("short?", "ExtractNullableInt16")]
    [InlineData("ushort", "ExtractUInt16")]
    [InlineData("ushort?", "ExtractNullableUInt16")]
    [InlineData("int", "ExtractInt32")]
    [InlineData("int?", "ExtractNullableInt32")]
    [InlineData("uint", "ExtractUInt32")]
    [InlineData("uint?", "ExtractNullableUInt32")]
    [InlineData("long", "ExtractInt64")]
    [InlineData("long?", "ExtractInt64")]
    [InlineData("ulong", "ExtractUInt64")]
    [InlineData("ulong?", "ExtractUInt64")]
    [InlineData("float", "ExtractFloat")]
    [InlineData("float?", "ExtractNullableFloat")]
    [InlineData("double", "ExtractDouble")]
    [InlineData("double?", "ExtractNullableDouble")]
    [InlineData("decimal", "ExtractDecimal")]
    [InlineData("decimal?", "ExtractNullableDecimal")]
    [InlineData("string", "ExtractString")]
    [InlineData("string?", "ExtractNullableString")]
    [InlineData("DateTime", "ExtractDateTime")]
    [InlineData("DateTime?", "ExtractNullableDateTime")]
    [InlineData("DateTimeOffset", "ExtractDateTimeOffset")]
    [InlineData("DateTimeOffset?", "ExtractNullableDateTimeOffset")]
    [InlineData("TimeSpan", "ExtractTimeSpan")]
    [InlineData("TimeSpan?", "ExtractNullableTimeSpan")]
    [InlineData("Guid", "ExtractGuid")]
    [InlineData("Guid?", "ExtractNullableGuid")]
    public void TableFunctionColumnOfTypeGeneratesExtractHelperMethod(string columnType, string expectedMethodName)
    {
        string source = BuildTableFunctionSource(columnType);

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];
        Assert.Empty(generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains(expectedMethodName, generatedCode, StringComparison.Ordinal);
    }
}
