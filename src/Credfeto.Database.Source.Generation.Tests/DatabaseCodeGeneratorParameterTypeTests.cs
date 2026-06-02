using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorParameterTypeTests : TestBase
{
    private static string BuildScalarFunctionWithParamSource(string parameterType)
    {
        return """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    PARAM_TYPE param,
                    CancellationToken cancellationToken);
            }
            """.Replace(oldValue: "PARAM_TYPE", newValue: parameterType, comparisonType: StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("int", "DbType.Int32")]
    [InlineData("long", "DbType.Int64")]
    [InlineData("short", "DbType.Int16")]
    [InlineData("byte", "DbType.Byte")]
    [InlineData("sbyte", "DbType.Byte")]
    [InlineData("char", "DbType.String")]
    [InlineData("decimal", "DbType.Decimal")]
    [InlineData("double", "DbType.Double")]
    [InlineData("float", "DbType.Single")]
    [InlineData("uint", "DbType.UInt32")]
    [InlineData("ulong", "DbType.UInt64")]
    [InlineData("ushort", "DbType.UInt16")]
    [InlineData("bool", "DbType.Boolean")]
    [InlineData("string", "DbType.String")]
    [InlineData("DateTime", "DbType.DateTime")]
    [InlineData("DateTimeOffset", "DbType.DateTimeOffset")]
    [InlineData("TimeSpan", "DbType.Time")]
    [InlineData("Guid", "DbType.Guid")]
    public void ScalarFunctionWithParameterSetsDbType(string parameterType, string expectedDbType)
    {
        string source = BuildScalarFunctionWithParamSource(parameterType);

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];
        Assert.Empty(generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains(expectedDbType, generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithByteArrayParameterSetsBinaryDbType()
    {
        const string source = """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        byte[] data,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];
        Assert.Empty(generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("DbType.Binary", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void NullableStringParameterGeneratesNullCheck()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    string? name,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("if (name is null)", generatedCode, StringComparison.Ordinal);
        Assert.Contains("DBNull.Value", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StringParameterSetsSizeFromLength()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    string name,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains(".Size = name.Length", generatedCode, StringComparison.Ordinal);
    }
}
