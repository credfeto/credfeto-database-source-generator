using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    [Fact]
    public Task NothingToGenerateGeneratesNothingAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public static class EmptyClass
        {
        }
    }";

        return VerifyAsync(code: test, Array.Empty<(string filename, string generated)>());
    }

    [Fact]
    public Task SimpleScalarFunctionAsync()
    {
        const string test = @"
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Credfeto.Database.Interfaces;

    namespace Credfeto.Database.Interfaces
    {
        public enum SqlObjectType
        {
            SCALAR_FUNCTION,
            TABLE_FUNCTION,
            STORED_PROCEDURE
        }

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class SqlObjectMapAttribute : Attribute
        {
            public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType)
            {
                this.Name = name;
                this.SqlObjectType = sqlObjectType;
            }

            public string Name { get; }

            public SqlObjectType SqlObjectType { get; }
        }
    }

    namespace ConsoleApplication1
    {
        public static partial class DatabaseWrapper
        {
            [SqlObjectMap(name: ""example.scalar"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
            public static partial Task<int> GetMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.EnumExtensions.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ConsoleApplication1;
")
        };

        return VerifyAsync(code: test, expected: expected);
    }
}