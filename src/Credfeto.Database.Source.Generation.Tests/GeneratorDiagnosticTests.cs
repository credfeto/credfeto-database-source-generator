using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class GeneratorDiagnosticTests : TestBase
{
    [Fact]
    public void CompilationHasNoNonCS8795Errors()
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
                    CancellationToken cancellationToken);
            }
            """;

        CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);

        // CS8795 is expected - partial method needs an implementation (the generator provides it)
        IReadOnlyList<Diagnostic> nonPartialErrors =
        [
            .. compilation
                .GetDiagnostics(Xunit.TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error && !StringComparer.Ordinal.Equals(d.Id, "CS8795")),
        ];

        Assert.Empty(nonPartialErrors);
    }

    [Fact]
    public void GeneratorRunResultHasOneResult()
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
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        Assert.Single(result.Results);
    }

    [Fact]
    public void GeneratorResultHasGeneratedSource()
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
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        GeneratorRunResult generatorResult = result.Results[0];

        IReadOnlyList<Diagnostic> allDiagnostics = [.. generatorResult.Diagnostics];
        string diagnosticsMessage = string.Join(
            separator: "; ",
            allDiagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")
        );

        Assert.True(
            generatorResult.GeneratedSources.Length > 0,
            userMessage: $"Expected at least 1 generated source but got 0. Diagnostics: [{diagnosticsMessage}]"
        );
    }
}
