using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Stuff.Infrastructure;
using Stuff.Mappers;
using Stuff.Models;
using Stuff.Primatives;

namespace Stuff;

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
}