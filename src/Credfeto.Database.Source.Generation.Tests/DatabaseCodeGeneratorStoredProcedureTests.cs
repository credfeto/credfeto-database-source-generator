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
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValuesAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: ""1.0.0"")]
    public static async partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Models.Account>> GetValuesAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        static IEnumerable<Models.Account> Extract(IDataReader reader)
        {
            int ordinalId = reader.GetOrdinal(name: nameof(Models.Account.Id));
            int ordinalName = reader.GetOrdinal(name: nameof(Models.Account.Name));
            int ordinalAddress = reader.GetOrdinal(name: nameof(Models.Account.Address));
            while (reader.Read())
            {
                yield return new Models.Account(
                                         Id: (int)reader.GetValue(ordinalId),
                                         Name: (string)reader.GetValue(ordinalName),
                                         Address: Mappers.AccountAddressMapper.MapFromDb(reader.GetValue(ordinalAddress)));
            }
        }
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""example.storedprocedure"";
        command.CommandType = CommandType.StoredProcedure;

        using (IDataReader reader = await command.ExecuteReaderAsync(behavior: CommandBehavior.Default, cancellationToken: cancellationToken))
        {
            return Extract(reader: reader).ToArray();
        }
    }
}
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
            (filename: "ConsoleApplication1.DatabaseWrapper.InsertAsyncAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: ""1.0.0"")]
    public static async partial System.Threading.Tasks.Task InsertAsyncAsync(System.Data.Common.DbConnection connection, string name, Primatives.AccountAddress address, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""example.storedprocedure"";
        command.CommandType = CommandType.StoredProcedure;
        DbParameter p0 = command.CreateParameter();
        p0.Value = name;
        p0.ParameterName = ""@name"";
        command.Parameters.Add(p0);
        DbParameter p1 = command.CreateParameter();
        Mappers.AccountAddressMapper.MapToDb(address, p1);
        p1.ParameterName = ""@address"";
        command.Parameters.Add(p1);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }
}