using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Credfeto.Database.Interfaces;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example.Mappers;

[SuppressMessage(
    category: "Microsoft.Performance",
    checkId: "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Unit test"
)]
[SuppressMessage(category: "ReSharper", checkId: "ClassNeverInstantiated.Local", Justification = "Unit test")]
internal sealed class AccountAddressMapper : IMapper<AccountAddress>
{
    public static AccountAddress MapFromDb(object value)
    {
        return new() { Value = (string)value };
    }

    public static void MapToDb(AccountAddress value, DbParameter parameter)
    {
        parameter.Value = value.Value;
        parameter.DbType = DbType.String;
        parameter.Size = 100;
    }
}
