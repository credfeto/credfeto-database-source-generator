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

    public Task<IReadOnlyList<Accounts>> GetAllAsync(AccountAddress accountAddress, CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetAllAsync(connection: c, address: accountAddress, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<Accounts?> GetAsync(int id, CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetAsync(connection: c, id: id, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task InsertAsync(string name, AccountAddress address, CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.InsertAsync(connection: c, name: name, address: address, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<int> GetMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetMeaningOfLifeAsync(connection: c, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<int?> GetOptionalMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetOptionalMeaningOfLifeAsync(connection: c, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<string> GetStringMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetStringMeaningOfLifeAsync(connection: c, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<AccountAddress> GetAddressMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetAddressMeaningOfLifeAsync(connection: c, cancellationToken: ct), cancellationToken: cancellationToken);
    }

    public Task<AccountAddress?> GetOptionalAddressMeaningOfLifeAsync(CancellationToken cancellationToken)
    {
        return this._database.ExecuteAsync(action: (c, ct) => DatabaseWrapper.GetOptionalAddressMeaningOfLifeAsync(connection: c, cancellationToken: ct), cancellationToken: cancellationToken);
    }
}