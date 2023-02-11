using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Credfeto.Database.Source.Generation.Example.Mappers;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example;

public static partial class Database
{
    [SqlObjectMap(name: "ethereum.account_getall", sqlObjectType: SqlObjectType.TABLE_FUNCTION)]
    public static partial Task<IReadOnlyList<Accounts>> GetAllAsync(DbConnection connection,
                                                                    [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address,
                                                                    CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.account_get", sqlObjectType: SqlObjectType.TABLE_FUNCTION)]
    public static partial Task<Accounts?> GetAsync(DbConnection connection, int id, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.account_insert", sqlObjectType: SqlObjectType.STORED_PROCEDURE)]
    public static partial Task InsertAsync(DbConnection connection, string name, [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
    public static partial Task<int> GetMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);
}