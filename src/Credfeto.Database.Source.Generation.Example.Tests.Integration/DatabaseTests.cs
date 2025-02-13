using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Bogus.DataSets;
using Credfeto.Database.Pgsql;
using Credfeto.Database.Source.Generation.Example.Models;
using FunFair.Test.Common;
using Microsoft.Extensions.Options;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Integration;

public sealed class DatabaseTests : TestBase
{
    private readonly ITestDatabase _dataSource;

    public DatabaseTests()
    {
        PgsqlServerConfiguration cfg = new(
            "Host=localhost;Username=markr;Password=ShhDontTellAnyone;Database=test"
        );
        IDatabase db = new PgsqlDatabase(Options.Create(cfg), this.GetTypedLogger<PgsqlDatabase>());
        this._dataSource = new TestDatabase(db);
    }

    [Fact]
    public async Task TableFunctionAsync()
    {
        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(60)))
        {
            IReadOnlyList<Accounts> result = await this._dataSource.GetAllAsync(
                new() { Value = "0x1234567890123456789012345678901234567890" },
                cancellationToken: cts.Token
            );
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task ScalarFunctionAsync()
    {
        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(60)))
        {
            int result = await this._dataSource.GetMeaningOfLifeAsync(cancellationToken: cts.Token);
            Assert.Equal(expected: 42, actual: result);
        }
    }

    [Fact]
    public async Task StoredProcedureAsync()
    {
        using (CancellationTokenSource cts = new(TimeSpan.FromSeconds(60)))
        {
            Identity name = MakeFake<Identity>(
                rules: static f =>
                    f.RuleFor(
                        property: u => u.Name,
                        setter: (faker, _) => faker.Name.FullName(faker.PickRandom<Name.Gender>())
                    ),
                itemCount: 1
            )[0];

            await this._dataSource.InsertAsync(
                name: name.Name,
                new() { Value = "0x1234567890123456789012345678901234567890" },
                cancellationToken: cts.Token
            );
        }
    }

    [SuppressMessage(
        category: "Microsoft.Performance",
        checkId: "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Unit test"
    )]
    [SuppressMessage(
        category: "ReSharper",
        checkId: "ClassNeverInstantiated.Local",
        Justification = "Unit test"
    )]
    private sealed class Identity
    {
        [SuppressMessage(
            category: "ReSharper",
            checkId: "UnusedAutoPropertyAccessor.Local",
            Justification = "Unit test"
        )]
        public required string Name { get; init; }
    }
}
