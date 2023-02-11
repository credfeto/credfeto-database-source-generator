using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus.DataSets;
using FunFair.Test.Common;
using Npgsql;
using Stuff;
using Xunit;
using Database = Stuff.Database;

namespace StuffTests;

public sealed class DatabaseTests : TestBase
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseTests()
    {
        this._dataSource = NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder("Host=localhost;Username=markr;Password=ShhDontTellAnyone;Database=test"));
    }

    [Fact]
    public async Task TableFunctionAsync()
    {
        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(60)))
        {
            NpgsqlConnection connection = await this._dataSource.OpenConnectionAsync(cts.Token);

            IReadOnlyList<Accounts> result = await Database.GetAllAsync(connection: connection, new() { Value = "0x1234567890123456789012345678901234567890" }, cancellationToken: cts.Token);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task StoredProcedureAsync()
    {
        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(60)))
        {
            NpgsqlConnection connection = await this._dataSource.OpenConnectionAsync(cts.Token);

            Identity name = MakeFake<Identity>(rules: f => f.RuleFor(property: u => u.Name, setter: (f, u) => f.Name.FirstName(f.PickRandom<Name.Gender>())), itemCount: 1)
                .First();

            await Database.InsertAsync(connection: connection, name: name.Name, new() { Value = "0x1234567890123456789012345678901234567890" }, cancellationToken: cts.Token);
        }
    }

    [SuppressMessage(category: "Microsoft.Performance", checkId: "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Unit test")]
    [SuppressMessage(category: "ReSharper", checkId: "ClassNeverInstantiated.Local", Justification = "Unit test")]
    private sealed class Identity
    {
        public string Name { get; } = default!;
    }
}