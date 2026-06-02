using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorStoredProcedureTests : TestBase
{
    [Fact]
    public void StoredProcedureWithGenericDialectGeneratesCallStatement()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.do_work", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                public static partial Task InsertAsync(
                    DbConnection connection,
                    string name,
                    CancellationToken cancellationToken);
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
        Assert.Contains("CALL dbo.do_work", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ExecuteNonQueryAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithMicrosoftSqlServerDialectGeneratesExecStatement()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.do_work", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                public static partial Task InsertAsync(
                    DbConnection connection,
                    string name,
                    CancellationToken cancellationToken);
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
        Assert.Contains("EXEC dbo.do_work", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithNoReturnGeneratesNonQueryExecution()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.delete_item", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                public static partial Task DeleteItemAsync(
                    DbConnection connection,
                    int id,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("ExecuteNonQueryAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithCollectionReturnTypeGeneratesReaderAndToArray()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                public record TestRow(int Id, string Name);

                [SqlObjectMap(name: "dbo.get_items", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<IReadOnlyList<TestRow>> GetItemsAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("ExecuteReaderAsync", generatedCode, StringComparison.Ordinal);
        Assert.Contains("CommandBehavior.Default", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ToArray", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithSingleRowReturnTypeGeneratesReaderAndFirstOrDefault()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                public record TestRow(int Id, string Name);

                [SqlObjectMap(name: "dbo.get_one_item", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<TestRow?> GetOneItemAsync(
                    DbConnection connection,
                    int id,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("CommandBehavior.SingleRow", generatedCode, StringComparison.Ordinal);
        Assert.Contains("FirstOrDefault", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureParameterIsIncludedInCommandText()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                [SqlObjectMap(name: "dbo.do_action", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                public static partial Task DoActionAsync(
                    DbConnection connection,
                    int itemId,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("@itemId", generatedCode, StringComparison.Ordinal);
    }
}
