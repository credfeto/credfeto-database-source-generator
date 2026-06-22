using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

// TABLE-VALUED PARAMETER (TVP) SOURCE GENERATOR TESTS
//
// These tests verify the source generator handles IReadOnlyList<T> parameters annotated with
// [SqlFieldMap<TMapper, IReadOnlyList<T>>] — the standard TVP pattern in this source generator.
//
// TVP pattern summary:
//   1. Define a mapper: class MyMapper : IMapper<IReadOnlyList<T>>
//   2. In MapToDb: cast DbParameter to SqlParameter, set SqlDbType.Structured, assign a DataTable
//   3. Annotate the stored procedure parameter: [SqlFieldMap<MyMapper, IReadOnlyList<T>>]
//
// The source generator emits a MapToDb call for each annotated parameter; the mapper handles
// the SQL Server-specific conversion at runtime.
public sealed class DatabaseCodeGeneratorTableValuedParameterTests : TestBase
{
    private const string TVP_MAPPER_SOURCE = """

        // TABLE-VALUED PARAMETER MAPPER
        public sealed class IntListMapper : IMapper<System.Collections.Generic.IReadOnlyList<int>>
        {
            public static System.Collections.Generic.IReadOnlyList<int> MapFromDb(object value)
                => throw new System.NotSupportedException("Cannot unmap a TVP");

            public static void MapToDb(
                System.Collections.Generic.IReadOnlyList<int> value,
                System.Data.Common.DbParameter parameter) { }
        }
        """;

    [Fact]
    public void StoredProcedureWithTableValuedParameterGeneratesMapToDbCall()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // TABLE-VALUED PARAMETER: ids is passed as a SQL Server TVP
                    [SqlObjectMap(name: "dbo.process_items", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task ProcessItemsAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> ids,
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
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithTableValuedParameterIncludesParameterNameInCommandText()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // TABLE-VALUED PARAMETER: accountIds is passed as a SQL Server TVP
                    [SqlObjectMap(name: "dbo.bulk_process", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task BulkProcessAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> accountIds,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("@accountIds", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithTableValuedParameterAndMicrosoftSqlDialectGeneratesExecStatement()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // TABLE-VALUED PARAMETER: itemIds is passed as a SQL Server TVP
                    [SqlObjectMap(name: "dbo.bulk_delete", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task BulkDeleteAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> itemIds,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("EXEC dbo.bulk_delete", generatedCode, StringComparison.Ordinal);
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithTableValuedParameterAndGenericDialectGeneratesCallStatement()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // TABLE-VALUED PARAMETER: tagIds is passed as a TVP via generic dialect
                    [SqlObjectMap(name: "dbo.tag_items", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                    public static partial Task TagItemsAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> tagIds,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("CALL dbo.tag_items", generatedCode, StringComparison.Ordinal);
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithNullableTableValuedParameterGeneratesNullCheck()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // NULLABLE TABLE-VALUED PARAMETER: optional TVP — null means no filter applied
                    [SqlObjectMap(name: "dbo.filter_items", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task FilterItemsAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int>? optionalIds,
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
        Assert.Contains("DBNull.Value", generatedCode, StringComparison.Ordinal);
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithTvpAndScalarParametersGeneratesBothParameters()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                internal static partial class TestWrapper
                {
                    // TVP MIXED WITH SCALAR: categoryId is scalar, itemIds is a TVP
                    [SqlObjectMap(name: "dbo.get_by_category_and_ids", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task GetByCategoryAndIdsAsync(
                        DbConnection connection,
                        int categoryId,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> itemIds,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("@categoryId", generatedCode, StringComparison.Ordinal);
        Assert.Contains("@itemIds", generatedCode, StringComparison.Ordinal);
        Assert.Contains("DbType.Int32", generatedCode, StringComparison.Ordinal);
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithTvpReturningCollectionGeneratesReaderAndToArray()
    {
        string source = $$"""
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                {{TVP_MAPPER_SOURCE}}

                public record ResultRow(int Id, string Name);

                internal static partial class TestWrapper
                {
                    // TVP RETURNING COLLECTION: passes TVP and gets rows back
                    [SqlObjectMap(name: "dbo.get_items_by_ids", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial ValueTask<IReadOnlyList<ResultRow>> GetItemsByIdsAsync(
                        DbConnection connection,
                        [SqlFieldMap<IntListMapper, IReadOnlyList<int>>] IReadOnlyList<int> ids,
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
        Assert.Contains("MapToDb", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ExecuteReaderAsync", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ToArray", generatedCode, StringComparison.Ordinal);
    }
}
