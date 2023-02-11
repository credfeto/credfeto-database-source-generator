using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stuff.Mappers;
using Stuff.Models;
using Stuff.Primatives;

namespace Stuff;

public static partial class Database
{
    [GeneratedCode(tool: "Stuff", version: "0.0.0.1")]
    public static async partial Task<Accounts?> GetAsync(DbConnection connection, int id, CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = "select * from ethereum.account_get(@id)";

        DbParameter p = command.CreateParameter();
        p.DbType = DbType.Int32;
        p.ParameterName = "id";
        p.Value = id;
        command.Parameters.Add(p);

        using (IDataReader reader = await command.ExecuteReaderAsync(behavior: CommandBehavior.SingleRow, cancellationToken: cancellationToken))
        {
            return ExtractAccountAddress(reader)
                .FirstOrDefault();
        }
    }

    [GeneratedCode(tool: "Stuff", version: "0.0.0.1")]
    public static async partial Task<IReadOnlyList<Accounts>> GetAllAsync(DbConnection connection, AccountAddress address, CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = "select * from ethereum.account_getall(@address)";

        DbParameter p = command.CreateParameter();
        AccountAddressMapper.MapToDb(thing: address, parameter: p);
        p.ParameterName = "address";
        command.Parameters.Add(p);

        using (IDataReader reader = await command.ExecuteReaderAsync(behavior: CommandBehavior.Default, cancellationToken: cancellationToken))
        {
            return ExtractAccountAddress(reader)
                .ToArray();
        }
    }

    [GeneratedCode(tool: "Stuff", version: "0.0.0.1")]
    public static partial Task InsertAsync(DbConnection connection, string name, AccountAddress address, CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = "ethereum.account_insert";
        command.CommandType = CommandType.StoredProcedure;

        DbParameter p1 = command.CreateParameter();
        p1.Value = name;
        p1.DbType = DbType.String;
        p1.ParameterName = "name";
        command.Parameters.Add(p1);

        DbParameter p2 = command.CreateParameter();
        AccountAddressMapper.MapToDb(thing: address, parameter: p2);
        p2.ParameterName = "address";
        command.Parameters.Add(p2);

        return command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IEnumerable<Accounts> ExtractAccountAddress(IDataReader reader)
    {
        int ordinalId = reader.GetOrdinal(nameof(Accounts.Id));
        int ordinalName = reader.GetOrdinal(nameof(Accounts.Name));
        int ordinalAddress = reader.GetOrdinal(nameof(Accounts.Address));

        while (reader.Read())
        {
            int recId = reader.GetInt32(ordinalId);
            string recName = reader.GetString(ordinalName);
            AccountAddress recAddress = AccountAddressMapper.MapFromDb(reader.GetValue(ordinalAddress));
            reader.GetValue(ordinalAddress);

            yield return new(Id: recId, Name: recName, Address: recAddress);
        }
    }

    public static async partial Task<int> GetMeaningOfLifeAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = "select ethereum.get_meaning_of_life_universe_and_everything()";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("No result returned.");
        }

        if (result is int value)
        {
            return value;
        }

        return Convert.ToInt32(value: result, provider: CultureInfo.InvariantCulture);
    }
}