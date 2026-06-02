using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorTableFunctionTests : TestBase
{
    [Fact]
    public void TableFunctionReturningCollectionGeneratesCode()
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

                [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
                    DbConnection connection,
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
        Assert.Contains("GetAllAsync", generatedCode, StringComparison.Ordinal);
        Assert.Contains("dbo.get_all", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ExecuteReaderAsync", generatedCode, StringComparison.Ordinal);
        Assert.Contains("CommandBehavior.Default", generatedCode, StringComparison.Ordinal);
        Assert.Contains("ToArray", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionReturningSingleRowGeneratesCode()
    {
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            internal static partial class TestWrapper
            {
                public record TestRow(int Id, string Name);

                [SqlObjectMap(name: "dbo.get_one", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<TestRow?> GetOneAsync(
                    DbConnection connection,
                    int id,
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
        Assert.Contains("CommandBehavior.SingleRow", generatedCode, StringComparison.Ordinal);
        Assert.Contains("FirstOrDefault", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionWithParameterGeneratesCommandParameter()
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

                [SqlObjectMap(name: "dbo.get_by_filter", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<IReadOnlyList<TestRow>> GetByFilterAsync(
                    DbConnection connection,
                    string filter,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("@filter", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionWithClassHavingProtectedConstructorSkipsProtectedConstructor()
    {
        // A table function returning a class with a protected constructor that has parameters.
        // The IsSameType local function filters out non-public constructors with parameters (line 276).
        // Note: static constructors cannot have parameters in C#, so line 271 is unreachable.
        const string source = """
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    public class TestRowWithProtectedCtor
                    {
                        protected TestRowWithProtectedCtor(string extra) { this.Id = 0; this.Name = extra; }
                        public TestRowWithProtectedCtor(int Id, string Name) { this.Id = Id; this.Name = Name; }
                        public int Id { get; }
                        public string Name { get; }
                    }

                    [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<TestRowWithProtectedCtor>> GetAllAsync(
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
        Assert.Contains("GetAllAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionWithClassHavingParameterlessConstructorSkipsIt()
    {
        // A table function returning a class with a public parameterless constructor alongside a
        // parameterized one. The GetColumns Where filter checks c.Parameters.Length > 0,
        // so the parameterless constructor is filtered out (covering the false branch of Length > 0 check).
        const string source = """
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    public class TestRowWithDefaultCtor
                    {
                        public TestRowWithDefaultCtor() { }
                        public TestRowWithDefaultCtor(int id, string name) { this.Id = id; this.Name = name; }
                        public int Id { get; }
                        public string Name { get; }
                    }

                    [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<TestRowWithDefaultCtor>> GetAllAsync(
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
        Assert.Contains("GetAllAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionGeneratesExtractLocalMethod()
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

                [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
                    DbConnection connection,
                    CancellationToken cancellationToken);
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("IEnumerable<", generatedCode, StringComparison.Ordinal);
        Assert.Contains("GetOrdinal", generatedCode, StringComparison.Ordinal);
        Assert.Contains("reader.Read()", generatedCode, StringComparison.Ordinal);
    }
}
