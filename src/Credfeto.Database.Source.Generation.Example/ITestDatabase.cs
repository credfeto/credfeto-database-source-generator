using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example;

public interface ITestDatabase
{
    ValueTask<IReadOnlyList<Accounts>> GetAllAsync(AccountAddress accountAddress, CancellationToken cancellationToken);

    ValueTask<Accounts?> GetAsync(int id, CancellationToken cancellationToken);

    ValueTask InsertAsync(string name, AccountAddress address, CancellationToken cancellationToken);

    ValueTask<int> GetMeaningOfLifeAsync(CancellationToken cancellationToken);

    ValueTask<int?> GetOptionalMeaningOfLifeAsync(CancellationToken cancellationToken);

    ValueTask<string> GetStringMeaningOfLifeAsync(CancellationToken cancellationToken);

    ValueTask<AccountAddress> GetAddressMeaningOfLifeAsync(CancellationToken cancellationToken);

    ValueTask<AccountAddress?> GetOptionalAddressMeaningOfLifeAsync(CancellationToken cancellationToken);
}