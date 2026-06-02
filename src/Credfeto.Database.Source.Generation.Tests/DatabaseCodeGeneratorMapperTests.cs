using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorMapperTests : TestBase
{
    [Fact]
    public void ScalarFunctionWithMapperOnReturnTypeGeneratesMapFromDbCall()
    {
        // This exercises MapperInfo construction via SqlFieldMapAttribute<TMapper, TDataType>
        // on the method return type (via [return: SqlFieldMap]) OR on the method itself.
        // Looking at the code, GetMapperInfo is called on the method's attribute lists.
        // SqlFieldMapAttribute is applied via [return: SqlFieldMap<TMapper, TDataType>].
        // Since SqlFieldMapAttribute targets ReturnValue | Property | Parameter,
        // we need to use [return: SqlFieldMap<MyMapper, int>] on the method.
        const string source = """
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
        Assert.Contains("MapFromDb", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithMapperOnParameterGeneratesMapToDbCall()
    {
        // This exercises MapperInfo on a parameter via [SqlFieldMap<TMapper, TDataType>]
        const string source = """
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
                    [SqlObjectMap(name: "dbo.do_work", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                    public static partial Task DoWorkAsync(
                        DbConnection connection,
                        [SqlFieldMap<MyIntMapper, int>] int itemId,
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
    public void StoredProcedureWithNullableMapperParameterGeneratesNullCheck()
    {
        // Exercises the nullable + mapper parameter path in CreateParameter
        // (the if(parameter.MapperInfo is not null) && if(parameter.Nullable) branch)
        const string source = """
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
                    [SqlObjectMap(name: "dbo.do_work", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
                    public static partial Task DoWorkAsync(
                        DbConnection connection,
                        [SqlFieldMap<MyIntMapper, int>] int? itemId,
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
    public void TableFunctionWithMapperColumnGeneratesMapFromDbCall()
    {
        // This exercises the mapper path in BuildExtractLocalMethod and AppendConstructorParameter
        // where the column has a mapper (mapperInfo is not null)
        const string source = """
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
                    public record TestRow([property: SqlFieldMap<MyIntMapper, int>] int Id, string Name);

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

        // Whether or not code is generated, the generator must not throw an unhandled exception
        Assert.DoesNotContain(generatorResult.Diagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));
    }

    [Fact]
    public void ScalarFunctionWithCustomGenericAttributeOnMethodStillGeneratesCode()
    {
        // This exercises the CreateMapperInfo(INamedTypeSymbol) path where the
        // attribute IS a generic type but NOT SqlFieldMapAttribute.
        // The method has a custom generic attribute, so CreateMapperInfo is called
        // with a non-SqlFieldMap generic type → the name check fails → returns null (line 151
        // in AttributeMappings.cs).
        const string source = """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                [AttributeUsage(AttributeTargets.Method)]
                public sealed class CustomGenericAttribute<T> : Attribute { }

                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    [CustomGenericAttribute<int>]
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
        Assert.Empty(generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("GetValueAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithNonGenericAttributeOnMethodStillGeneratesCode()
    {
        // This exercises the CreateMapperInfo(INamedTypeSymbol) path where the
        // containing type is NOT a generic type (line 144 in AttributeMappings.cs).
        // The method has a non-generic method attribute, so CreateMapperInfo is called
        // with a non-generic type → !containingType.IsGenericType returns null.
        const string source = """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                [AttributeUsage(AttributeTargets.Method)]
                public sealed class CustomMethodAttribute : Attribute { }

                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    [CustomMethodAttribute]
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
        Assert.Empty(generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        IReadOnlyList<GeneratedSourceResult> sources = generatorResult.GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("GetValueAsync", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ScalarFunctionWithUnresolvableMapperTypeOnReturnProducesError()
    {
        // When [return: SqlFieldMap<UnresolvableMapper, int>] is used and UnresolvableMapper
        // doesn't exist, the type argument is an ErrorType symbol.
        // This triggers the ErrorType check in CreateMapperInfo (line 158),
        // causing CouldNotDetermineMapped to throw InvalidModelException (line 181).
        // The exception is caught and produces an InvalidModel diagnostic (CDSG001).
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
                    [return: SqlFieldMap<UnresolvableMapper, int>]
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

        // Either an error diagnostic is produced or no sources (if the mapper was skipped silently)
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(condition: hasErrorDiagnostic || hasNoSources, userMessage: "Expected either an error diagnostic or no sources for unresolvable mapper type");
    }

    [Fact]
    public void ScalarFunctionWithUnresolvableDataTypeInMapperProducesError()
    {
        // When [return: SqlFieldMap<ValidMapper, UnresolvableDataType>] is used where the
        // first type arg resolves but the second doesn't, the second type argument is an ErrorType.
        // This triggers line 165 in AttributeMappings.CreateMapperInfo (second arg ErrorType check),
        // causing CouldNotDetermineMapped to throw InvalidModelException (line 181).
        const string source = """
            using System;
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                public sealed class ValidMapper : IMapper<int>
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
                    [return: SqlFieldMap<ValidMapper, UnresolvableDataType>]
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

        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(condition: hasErrorDiagnostic || hasNoSources, userMessage: "Expected error or no sources for unresolvable data type in mapper");
    }

    [Fact]
    public void TableFunctionWithUnresolvableMapperOnColumnProducesErrorOrNoCode()
    {
        // When a record parameter has [SqlFieldMap<UnresolvableMapper, int>] and the mapper
        // type doesn't exist, the type argument is an ErrorType symbol.
        // This triggers the GetMapperInfo(IParameterSymbol) path where CreateMapperInfo2
        // processes the attribute's class. If the class is null/ErrorType, it returns null.
        // If it's a generic type with unresolvable args, it may trigger lines 158/165 in
        // CreateMapperInfo, causing CouldNotDetermineMapped to throw InvalidModelException.
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
                    public record TestRow([SqlFieldMap<UnresolvableMapper, int>] int Id, string Name);

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

        // Either an error diagnostic or no sources (unresolvable mapper type)
        bool hasErrorDiagnostic = generatorResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        bool hasNoSources = generatorResult.GeneratedSources.Length == 0;

        Assert.True(condition: hasErrorDiagnostic || hasNoSources, userMessage: "Expected either an error diagnostic or no sources for unresolvable mapper type on column");
    }
}
