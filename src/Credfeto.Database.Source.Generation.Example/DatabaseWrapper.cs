using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Credfeto.Database.Source.Generation.Example.Mappers;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example;

internal static partial class DatabaseWrapper
{
    [SqlObjectMap(name: "ethereum.account_getall", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<IReadOnlyList<Accounts>> GetAllAsync(DbConnection connection,
                                                                         [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address,
                                                                         CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.account_get", sqlObjectType: SqlObjectType.TABLE_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<Accounts?> GetAsync(DbConnection connection, int id, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.account_get", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.MICROSOFT_SQL_SERVER)]
    public static partial ValueTask<Accounts?> GetMssqlAsync(DbConnection connection, int id, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.account_insert", sqlObjectType: SqlObjectType.STORED_PROCEDURE, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask InsertAsync(DbConnection connection, string name, [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<int> GetMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<int?> GetOptionalMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<string> GetStringMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    [return: SqlFieldMap<AccountAddressMapper, AccountAddress>]
    public static partial ValueTask<AccountAddress> GetAddressMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);

    [SqlObjectMap(name: "ethereum.get_meaning_of_life_universe_and_everything", sqlObjectType: SqlObjectType.SCALAR_FUNCTION, sqlDialect: SqlDialect.GENERIC)]
    [return: SqlFieldMap<AccountAddressMapper, AccountAddress>]
    public static partial ValueTask<AccountAddress?> GetOptionalAddressMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken);
}