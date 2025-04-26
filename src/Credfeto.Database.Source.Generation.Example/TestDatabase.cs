using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example;

public sealed class TestDatabase : ITestDatabase
{
    private readonly IDatabase _database;

    public TestDatabase(IDatabase database)
    {
        this._database = database;
    }

    public ValueTask<IReadOnlyList<Accounts>> GetAllAsync(
        AccountAddress accountAddress,
        CancellationToken cancellationToken
    )
    {
        return this._database.ExecuteAsync(
            action: (c, ct) =>
                DatabaseWrapper.GetAllAsync(connection: c, address: accountAddress, cancellationToken: ct),
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<Accounts?> GetAsync(int id, CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: (c, ct) => DatabaseWrapper.GetAsync(connection: c, id: id, cancellationToken: ct),
            cancellationToken: cancellationToken
        );
    }

    public ValueTask InsertAsync(string name, AccountAddress address, CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: (c, ct) =>
                DatabaseWrapper.InsertAsync(connection: c, name: name, address: address, cancellationToken: ct),
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<int> GetMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: DatabaseWrapper.GetMeaningOfLifeAsync,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<int?> GetOptionalMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: DatabaseWrapper.GetOptionalMeaningOfLifeAsync,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<string> GetStringMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: DatabaseWrapper.GetStringMeaningOfLifeAsync,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<AccountAddress> GetAddressMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: DatabaseWrapper.GetAddressMeaningOfLifeAsync,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<AccountAddress?> GetOptionalAddressMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(
            action: DatabaseWrapper.GetOptionalAddressMeaningOfLifeAsync,
            cancellationToken: cancellationToken
        );
    }
}
