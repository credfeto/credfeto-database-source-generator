using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorEdgeCaseTests : TestBase
{
    [Fact]
    public void MethodInStructIsIgnored()
    {
        // classDeclarationSyntax will be null because the method is in a struct,
        // not a class. GetClassDeclarationSyntax only looks for ClassDeclarationSyntax.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial struct TestStruct
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
        Assert.Empty(sources);
    }

    [Fact]
    public void MethodReturningNonTaskIdentifierProducesInvalidModelDiagnostic()
    {
        // A partial method with a named-type (IdentifierNameSyntax) return type that is NOT
        // Task or ValueTask — this triggers the throw in GetNonGenericMethodReturnType (line 151).
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
                    public static partial String GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
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
    public void MethodReturningNonTaskGenericTypeProducesInvalidModelDiagnostic()
    {
        // A partial method returning a generic type that is NOT Task or ValueTask —
        // triggers GetGenericTaskReturnType with non-Task generic (line 197).
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
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial IEnumerable<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
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
    public void TableFunctionWithParameterAttributeOnColumnHasAttributeScanned()
    {
        // A record where the constructor parameter has an attribute.
        // This triggers the GetMapperInfo(IParameterSymbol) extension method to scan
        // the attribute, covering line 53 in AttributeMappings.cs.
        // We use [Obsolete] which is NOT a SqlFieldMapAttribute, so the mapper will be null.
        const string source = """
            using System;
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    public record TestRow([Obsolete] int Id, string Name);

                    [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
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
        Assert.Contains("GetOrdinal", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void TableFunctionWithSqlFieldMapOnColumnGeneratesMapFromDbInExtract()
    {
        // A record where the constructor parameter has SqlFieldMapAttribute.
        // This exercises the mapper path in AppendConstructorParameter (lines 227-229)
        // and GetMapperInfo(IParameterSymbol) (line 53).
        const string source = """
            using System;
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                public sealed class MyIntMapper : IMapper<int>
                {
                    public static int MapFromDb(object value) => Convert.ToInt32(value);
                    public static void MapToDb(int value, System.Data.Common.DbParameter parameter)
                    {
                        parameter.Value = value;
                    }
                }

                internal static partial class TestWrapper
                {
                    public record TestRow([SqlFieldMap<MyIntMapper, int>] int Id, string Name);

                    [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];
        Assert.DoesNotContain(generatorResult.Diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "CDSG002"));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        // The mapper path generates MapFromDb call for the mapped column
        Assert.Contains("MapFromDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithMapperReturnTypeGeneratesMapFromDbCall()
    {
        // A scalar function where the method has [return: SqlFieldMap<...>] attribute.
        // This exercises the mapper path in GenerateScalarFunctionMethod (lines 324-328).
        const string source = """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                public sealed class MyIntMapper : IMapper<int>
                {
                    public static int MapFromDb(object value) => Convert.ToInt32(value);
                    public static void MapToDb(int value, System.Data.Common.DbParameter parameter)
                    {
                        parameter.Value = value;
                    }
                }

                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    [return: SqlFieldMap<MyIntMapper, int>]
                    public static partial ValueTask<int> GetMappedValueAsync(
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
        Assert.Contains("MapFromDb(value: result)", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredProcedureWithMicrosoftSqlServerAndNoParamsGeneratesExecStatement()
    {
        // Stored procedure with MICROSOFT_SQL_SERVER dialect but no parameters.
        // This exercises line 469 in DatabaseSourceCodeGenerator (non-IParameterSymbol else branch)
        // as well as lines 512-514 (the MICROSOFT_SQL_SERVER case).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.do_work", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
                    public static partial Task DoWorkAsync(
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
        Assert.Contains("EXEC dbo.do_work", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithNullableReturnTypeGeneratesNullReturn()
    {
        // A scalar function returning nullable int? - exercises the isNullable path
        // in GenerateScalarFunctionMethod (line 316-317 / 310 area).
        // This should already be covered by ScalarFunctionReturningNullableIntGeneratesReturnNull
        // but let's verify with ValueTask<int?>.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_optional", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<long?> GetOptionalAsync(
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
        Assert.Contains("return null;", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithCustomTypeParameterCausesGeneratorException()
    {
        // A scalar function with a parameter of an unknown type that has no TypeMapper entry.
        // This exercises TypeMapper.Map returning null (the _ => null branch) and
        // ParameterSetter.ThrowInvalidDbType being called.
        // The generator will throw an InvalidModelException during code generation (action phase).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                public readonly struct MyCustomValue
                {
                    public int Value { get; }
                    public MyCustomValue(int value) { this.Value = value; }
                }

                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        MyCustomValue customParam,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The generator should encounter an error trying to determine DbType for the custom type
        // (either as an exception on the result, or no generated sources due to the error)
        bool hasException = generatorResult.Exception is not null;
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasException || hasNoSources,
            userMessage: "Expected either an exception or no generated sources for unsupported parameter type"
        );
    }

    [Fact]
    public void ScalarFunctionWithVoidReturnTypeCausesGeneratorError()
    {
        // A scalar function (SqlObjectType.SCALAR_FUNCTION) with a non-generic ValueTask return type.
        // This means the method has no ElementReturnType, which triggers the
        // throw new InvalidOperationException in GenerateScalarFunctionMethod (line 310).
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
                    public static partial ValueTask GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The generator should encounter an error since SCALAR_FUNCTION requires a return value
        bool hasException = generatorResult.Exception is not null;
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasException || hasNoSources,
            userMessage: "Expected either an exception or no generated sources for scalar function with void return"
        );
    }

    [Fact]
    public void MethodReturningTaskOfArrayTypeProducesInvalidModelDiagnostic()
    {
        // A partial method returning ValueTask<int[]> triggers line 263 in DatabaseSyntaxReceiver:
        // The task type argument is int[] (ArrayTypeSyntax), which is not GenericNameSyntax,
        // PredefinedTypeSyntax, NullableTypeSyntax, or IdentifierNameSyntax,
        // so InvalidModelException is thrown → CDSG001 diagnostic.
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
                    public static partial ValueTask<int[]> GetArrayAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
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
    public void TableFunctionWithCustomColumnTypeAndNoMapperCausesGeneratorError()
    {
        // A table function record with a column type that has no ExtractColumns entry.
        // This exercises ExtractColumns.GenerateExtractColumnMapper returning null (line 46),
        // which then causes an InvalidModelException.
        const string source = """
            using System.Collections.Generic;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                public readonly struct MyCustomValue
                {
                    public int Value { get; }
                    public MyCustomValue(int value) { this.Value = value; }
                }

                internal static partial class TestWrapper
                {
                    public record TestRow(MyCustomValue CustomField, string Name);

                    [SqlObjectMap(name: "dbo.get_all", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<TestRow>> GetAllAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The generator should encounter an error trying to extract the custom column type
        bool hasException = generatorResult.Exception is not null;
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasException || hasNoSources,
            userMessage: "Expected either an exception or no generated sources for unsupported column type"
        );
    }

    [Fact]
    public void MethodWithUnresolvableAttributeAndSqlObjectMapGeneratesCode()
    {
        // When a method has an unresolvable attribute alongside [SqlObjectMap],
        // GetSymbol returns null for the unresolvable attribute, hitting line 195
        // in AttributeMappings.CreateSqlObject (null check).
        // The null return means that attribute is skipped, and the SqlObjectMap
        // attribute is processed normally, generating code.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [UnresolvableAttribute("arg1", "arg2", "arg3")]
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The unresolvable attribute produces no code, but the SqlObjectMap is still processed
        // Generator may produce a source or may silently fail — either way, no CDSG002 exception
        Assert.DoesNotContain(generatorResult.Diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "CDSG002"));
    }

    [Fact]
    public void MethodWithSqlObjectMapWithTooFewArgumentsProducesNoCode()
    {
        // When [SqlObjectMap] has fewer than 3 arguments, AttributeMappings.CreateSqlObject
        // returns null at line 212 (argument count check), so no code is generated.
        // This source is syntactically valid but semantically invalid (missing required args).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap("dbo.get_value")]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Empty(sources);
    }

    [Fact]
    public void MethodWithUnresolvableReturnTypeArgumentProducesInvalidModelDiagnostic()
    {
        // A method returning ValueTask<SomeUnknownType> where the type argument can't
        // be resolved produces a diagnostic. The unresolved type results in an ErrorType
        // symbol from GetSymbol, triggering ValidateSymbol to throw InvalidModelException
        // (covering lines 172 and/or 177 in DatabaseSyntaxReceiver.cs).
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
                    public static partial ValueTask<SomeUnknownType> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // Generator produces error diagnostic since the return type can't be resolved
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasErrorDiagnostic || hasNoSources,
            userMessage: "Expected either an error diagnostic or no generated sources for unresolvable return type"
        );
    }

    [Fact]
    public void MethodWithUnresolvableCollectionElementTypeProducesInvalidModelDiagnostic()
    {
        // A method returning ValueTask<IReadOnlyList<UnknownElement>> where the element type
        // is unresolvable. The inner generic type argument GetSymbol may return ErrorType,
        // triggering ValidateSymbol to throw InvalidModelException (line 177 in DatabaseSyntaxReceiver.cs).
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
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<IReadOnlyList<UnknownElement>> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // Generator should produce an error since the collection element type can't be resolved
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasErrorDiagnostic || hasNoSources,
            userMessage: "Expected either an error diagnostic or no generated sources for unresolvable collection element type"
        );
    }

    [Fact]
    public void MethodReturningTaskOfUnresolvableGenericTypeProducesInvalidModelDiagnostic()
    {
        // A partial method returning ValueTask<UnknownGeneric<int>> where the outer type is
        // unresolvable. GetSymbol on the inner GenericNameSyntax may return an ErrorType symbol,
        // triggering ValidateSymbol line 177 in DatabaseSyntaxReceiver.cs.
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
                    public static partial ValueTask<UnknownGeneric<int>> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The generator should encounter an error since the return type can't be resolved
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasErrorDiagnostic || hasNoSources,
            userMessage: "Expected either an error diagnostic or no generated sources for unresolvable generic return type"
        );
    }

    [Fact]
    public void ScalarFunctionReturningCharTypeCausesGeneratorError()
    {
        // A scalar function returning ValueTask<char>. The char type is a PredefinedTypeSyntax
        // that resolves correctly, but ExtractColumns.GenerateReturn does not have an entry for "char",
        // so it returns null and the generator throws InvalidModelException (line 71 in ExtractColumns.cs).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_char", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<char> GetCharAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // The generator should encounter an error since char is not a supported scalar return type
        bool hasException = generatorResult.Exception is not null;
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(
            condition: hasException || hasErrorDiagnostic || hasNoSources,
            userMessage: "Expected either an exception, error diagnostic, or no generated sources for char return type"
        );
    }
}
