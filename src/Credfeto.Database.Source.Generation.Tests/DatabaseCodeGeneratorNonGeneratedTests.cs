using System.Collections.Generic;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorNonGeneratedTests : TestBase
{
    [Fact]
    public void NonPartialMethodIsIgnored()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken)
                {
                    return default;
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Empty(sources);
    }

    [Fact]
    public void MethodWithNoAttributeIsIgnored()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;

            internal static partial class TestWrapper
            {
                public static partial ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Empty(sources);
    }

    [Fact]
    public void MethodInNonPartialClassIsIgnored()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static class TestWrapper
            {
                [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<int> GetValueAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Empty(sources);
    }

    [Fact]
    public void EmptySourceProducesNoGeneratedFiles()
    {
        const string source = "// empty file";

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Empty(sources);
    }
}
