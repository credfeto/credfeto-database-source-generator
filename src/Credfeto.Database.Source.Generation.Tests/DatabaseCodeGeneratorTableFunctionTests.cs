using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorTableFunctionTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    [Fact]
    public Task SimpleScalarFunctionGetReadOnlyListOfAccountsAsync()
    {
        const string test = @"
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Credfeto.Database.Interfaces;
    using Primatives;
    using Mappers;
    using Models;

    " + Constants.DatabaseTypes + Constants.AccountAddressClass + Constants.AccountAddressMapperClass + Constants.AccountModelClass + @"

    namespace ConsoleApplication1
    {
        public static partial class DatabaseWrapper
        {
            [SqlObjectMap(name: ""example.tablefunction"", sqlObjectType: SqlObjectType.TABLE_FUNCTION)]
            public static partial Task<IReadOnlyList<Account>> GetValuesAsync(DbConnection connection, CancellationToken cancellationToken);
        }
    }";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValuesAsync.generated.cs", generated: @"using System;
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