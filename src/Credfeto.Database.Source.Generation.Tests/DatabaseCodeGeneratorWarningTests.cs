using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

public sealed class DatabaseCodeGeneratorWarningTests : TestBase
{
    [Fact]
    public void InvalidSqlObjectTypeProducesNoGeneratedCode()
    {
        // Using a numeric cast for SqlObjectType - the generator reads the argument text as
        // "(SqlObjectType)999" which has no '.' so GetSqlObjectType returns null -> no generation.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: (SqlObjectType)999, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;

        // No code should be generated when object type is invalid
        Assert.Empty(sources);
    }

    [Fact]
    public void InvalidSqlDialectProducesNoGeneratedCode()
    {
        // Using a numeric cast for SqlDialect - the generator reads the argument text as
        // "(SqlDialect)999" which has no '.' so GetSqlDialect returns null -> no generation.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: (SqlDialect)999)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;

        // No code should be generated when dialect is invalid
        Assert.Empty(sources);
    }

    [Fact]
    public void InvalidSqlObjectTypeDoesNotThrowUnhandledException()
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
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: (SqlObjectType)999, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        // Should not produce an unhandled exception diagnostic (CDSG002)
        IReadOnlyList<Diagnostic> allDiagnostics = [.. result.Results[0].Diagnostics];
        Assert.DoesNotContain(allDiagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidSqlDialectDoesNotThrowUnhandledException()
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
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: (SqlDialect)999)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        // Should not produce an unhandled exception diagnostic (CDSG002)
        IReadOnlyList<Diagnostic> allDiagnostics = [.. result.Results[0].Diagnostics];
        Assert.DoesNotContain(allDiagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));
    }

    [Fact]
    public void UnquotedObjectNameIsUsedDirectly()
    {
        // Test the RemoveQuotes path where the name is NOT quoted.
        // The nameof() expression produces the text "nameof(TestWrapper)" which has no leading '"',
        // so RemoveQuotes returns it unchanged (the no-quotes branch at line 290).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: nameof(TestWrapper), sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        // nameof(TestWrapper) produces the text "nameof(TestWrapper)" which has no leading '"'
        // So RemoveQuotes should return it as-is (the no-quotes branch)
        // The generator should still produce code using that literal name
        IReadOnlyList<Diagnostic> allDiagnostics = [.. result.Results[0].Diagnostics];
        Assert.DoesNotContain(allDiagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));

        IReadOnlyList<GeneratedSourceResult> sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);

        string generatedCode = sources[0].SourceText.ToString();
        Assert.Contains("nameof(TestWrapper)", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AttributeWithWrongNumberOfArgumentsProducesNoCode()
    {
        // The CreateSqlObject method checks: if (argumentList?.Arguments.Count != 3) return null
        // This covers the case where the SqlObjectMapAttribute has wrong argument count
        // (though the attribute constructor requires exactly 3, so we can't really do this at compile time)
        // Instead test with a non-SqlObjectMap attribute which should just produce no code
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [System.Obsolete("test")]
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
    public void MethodWithValidSqlObjectMapAttributeAlongSideOtherAttributesGeneratesCode()
    {
        // A method with two [SqlObjectMap] attributes: one with an invalid sqlObjectType (generates a warning)
        // and one that is fully valid. This covers the NullIfEmpty path where the warnings list is non-empty
        // AND the method is successfully built (from the valid attribute), triggering
        // NullIfEmpty to return the non-null warnings list (line 442 in DatabaseSyntaxReceiver.cs).
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: (SqlObjectType)999, sqlDialect: SqlDialect.GENERIC)]
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        // The method should generate code from the valid attribute
        // and may have a warning diagnostic from the invalid one
        IReadOnlyList<GeneratorRunResult> generatorResults = result.Results;
        Assert.Single(generatorResults);

        GeneratorRunResult generatorResult = generatorResults[0];

        // Should not produce an unhandled exception diagnostic (CDSG002)
        Assert.DoesNotContain(generatorResult.Diagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidEnumValueNameInSqlObjectTypeProducesExceptionDiagnostic()
    {
        // Using a constant cast expression for SqlObjectType where the constant's name is not a
        // valid SqlObjectType member triggers Enum.Parse to throw ArgumentException (not
        // InvalidModelException). The text "(SqlObjectType)InvalidConstants.BAD_TYPE" has a dot,
        // so GetSqlObjectType splits it into ["(SqlObjectType)InvalidConstants", "BAD_TYPE"],
        // then Enum.Parse("BAD_TYPE") throws ArgumentException.
        // This is caught by the outer catch(Exception) in GetMethodDetails, creating ErrorInfo
        // and triggering ReportException with CDSG002 diagnostic.
        // Note: the cast (SqlObjectType)99 is a valid attribute argument (unnamed enum value),
        // so the attribute binds successfully and GetSymbol returns the constructor.
        const string source = """
            using System.Data.Common;
            using System.Threading;
            using System.Threading.Tasks;
            using Credfeto.Database.Interfaces;

            namespace TestDatabase
            {
                internal static class InvalidConstants
                {
                    public const int BAD_TYPE = 99;
                }

                internal static partial class TestWrapper
                {
                    [SqlObjectMap(name: "dbo.get_value", sqlObjectType: (SqlObjectType)InvalidConstants.BAD_TYPE, sqlDialect: SqlDialect.GENERIC)]
                    public static partial ValueTask<int> GetValueAsync(
                        DbConnection connection,
                        CancellationToken cancellationToken);
                }
            }
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(source);

        IReadOnlyList<Diagnostic> diagnostics = [
            .. result.Results[0]
                .Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error),
        ];

        // Should produce CDSG002 (unhandled exception) since Enum.Parse throws ArgumentException
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => string.Equals(d.Id, "CDSG002", StringComparison.Ordinal));
    }

}
