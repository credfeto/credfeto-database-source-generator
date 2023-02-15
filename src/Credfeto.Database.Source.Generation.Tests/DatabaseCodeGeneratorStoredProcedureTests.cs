using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorStoredProcedureTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    public DatabaseCodeGeneratorStoredProcedureTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public Task SimpleStoredProcedureGetReadOnlyListOfAccountsAsync()
    {
        const string test = @"
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Primatives;
using Mappers;
using Models;

#nullable enable

" + Constants.DatabaseTypes + Constants.AccountAddressClass + Constants.AccountAddressMapperClass + Constants.AccountModelClass + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.storedprocedure"", sqlObjectType: SqlObjectType.STORED_PROCEDURE)]
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

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleStoredProcedureExecNoResultsAsync()
    {
        const string test = @"
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Primatives;
using Mappers;
using Models;

#nullable enable

" + Constants.DatabaseTypes + Constants.AccountAddressClass + Constants.AccountAddressMapperClass + Constants.AccountModelClass + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.storedprocedure"", sqlObjectType: SqlObjectType.STORED_PROCEDURE)]
        public static partial Task InsertAsyncAsync(DbConnection connection,
                    string name,
                    [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                    AccountAddress address,
                    CancellationToken cancellationToken);
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

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }
}