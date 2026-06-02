using System;
using System.Collections.Generic;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorReturnTypeTests : TestBase
{
    private static string BuildScalarFunctionSource(string returnType)
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
                public static partial ValueTask<RETURN_TYPE> GetValueAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """.Replace(oldValue: "RETURN_TYPE", newValue: returnType, comparisonType: StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("bool", "Convert.ToBoolean")]
    [InlineData("byte", "Convert.ToByte")]
    [InlineData("sbyte", "Convert.ToSByte")]
    [InlineData("short", "Convert.ToInt16")]
    [InlineData("ushort", "Convert.ToUInt16")]
    [InlineData("int", "Convert.ToInt32")]
    [InlineData("uint", "Convert.ToUInt32")]
    [InlineData("long", "Convert.ToInt64")]
    [InlineData("ulong", "Convert.ToUInt64")]
    [InlineData("decimal", "Convert.ToDecimal")]
    [InlineData("double", "Convert.ToDouble")]
    [InlineData("float", "Convert.ToDouble")]
    [InlineData("string", "(string)")]
    [InlineData("DateTime", "Convert.ToDateTime")]
    [InlineData("DateTimeOffset", "(System.DateTimeOffset)")]
    [InlineData("TimeSpan", "(System.TimeSpan)")]
    [InlineData("Guid", "(System.Guid)")]
    public void ScalarFunctionReturningTypeGeneratesExpectedConvert(string returnType, string expectedConvertCall)
    {
        string source = BuildScalarFunctionSource(returnType);

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains(expectedConvertCall, generatedCode, StringComparison.Ordinal);
    }
}
