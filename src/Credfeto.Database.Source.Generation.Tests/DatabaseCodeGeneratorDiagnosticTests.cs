using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorDiagnosticTests : TestBase
{
    [Fact]
    public void MethodNotReturningTaskProducesInvalidModelDiagnostic()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial int GetValueAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<Diagnostic> diagnostics =
        [
            .. result.Results[0].Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error),
        ];

        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "CDSG001"));
    }

    [Fact]
    public void GeneratedFilesHaveCorrectFileNameFormat()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestNamespace
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string hintName = sources[0].HintName;
        Assert.Contains("TestNamespace.TestWrapper.GetValueAsync.Database.", hintName, StringComparison.Ordinal);
        Assert.EndsWith(".generated.cs", hintName, StringComparison.Ordinal);
    }

    [Fact]
    public void MultipleMethodsGenerateMultipleFiles()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value1", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValue1Async(
                    DbConnection connection,
                    CancellationToken cancellationToken);

                [SqlObjectMap(name: "dbo.get_value2", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValue2Async(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Equal(expected: 2, actual: sources.Count);
    }
}
