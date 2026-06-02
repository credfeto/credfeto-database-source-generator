using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorAccessModifierTests : TestBase
{
    [Fact]
    public void InternalMethodGeneratesInternalKeyword()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    internal static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
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
        Assert.Contains("internal", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ProtectedMethodGeneratesProtectedKeyword()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal partial class TestBaseWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    protected partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
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
        Assert.Contains("protected", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ProtectedInternalMethodGeneratesProtectedInternalKeywords()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal partial class TestBaseWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    protected internal partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
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
        Assert.Contains("protected internal", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PrivateMethodGeneratesPrivateKeyword()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal partial class TestBaseWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    private partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
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
        Assert.Contains("private", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void NonStaticClassGeneratesNonStaticPartialClass()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal partial class TestBaseWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();

        // Should NOT contain "static partial class" since the class is not static
        Assert.DoesNotContain("static partial class", generatedCode, StringComparison.Ordinal);
        Assert.Contains("partial class", generatedCode, StringComparison.Ordinal);
    }
}
