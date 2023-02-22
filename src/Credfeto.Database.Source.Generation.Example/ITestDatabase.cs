using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example;

public interface ITestDatabase
{
    Task<IReadOnlyList<Accounts>> GetAllAsync(AccountAddress accountAddress, CancellationToken cancellationToken);

    Task<Accounts?> GetAsync(int id, CancellationToken cancellationToken);

    Task InsertAsync(string name, AccountAddress address, CancellationToken cancellationToken);

    Task<int> GetMeaningOfLifeAsync(CancellationToken cancellationToken);

    Task<int?> GetOptionalMeaningOfLifeAsync(CancellationToken cancellationToken);

    Task<string> GetStringMeaningOfLifeAsync(CancellationToken cancellationToken);

    Task<AccountAddress> GetAddressMeaningOfLifeAsync(CancellationToken cancellationToken);

    Task<AccountAddress?> GetOptionalAddressMeaningOfLifeAsync(CancellationToken cancellationToken);
}