using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Credfeto.Database.Interfaces;
using Credfeto.Database.Source.Generation.Example.Primatives;
using Microsoft.Data.SqlClient;

namespace Credfeto.Database.Source.Generation.Example.Mappers;

// TABLE-VALUED PARAMETER (TVP) MAPPER
// Converts IReadOnlyList<AccountId> to a SQL Server table-valued parameter.
// The SQL Server user-defined table type must be created in the database:
//   CREATE TYPE dbo.AccountId AS TABLE (Id BIGINT NOT NULL)
// Annotate stored procedure parameters with:
//   [SqlFieldMap<AccountIdListMapper, IReadOnlyList<AccountId>>]
public sealed class AccountIdListMapper : IMapper<IReadOnlyList<AccountId>>
{
    private const string TABLE_TYPE = "dbo.AccountId";
    private const string COLUMN_NAME = "Id";

    public static IReadOnlyList<AccountId> MapFromDb(object value)
    {
        throw new System.NotSupportedException("Cannot unmap a table-valued parameter from the database.");
    }

    public static void MapToDb(IReadOnlyList<AccountId> value, DbParameter parameter)
    {
        DataTable records = CreateTableHeader();

        AddRows(rows: value, records: records);

        SqlParameter tvpParam = (SqlParameter)parameter;
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = TABLE_TYPE;
        tvpParam.Value = records;
    }

    private static void AddRows(IReadOnlyList<AccountId> rows, DataTable records)
    {
        foreach (AccountId accountId in rows)
        {
            DataRow row = records.NewRow();
            row[COLUMN_NAME] = accountId.Value;
            records.Rows.Add(row);
        }
    }

    [SuppressMessage(
        category: "SmartAnalyzers.CSharpExtensions.Annotations",
        checkId: "CSE007: Handle disposal correctly",
        Justification = "Disposed by owner"
    )]
    private static DataTable CreateTableHeader()
    {
        DataTable records = new(TABLE_TYPE);
        records.Columns.Add(columnName: COLUMN_NAME, type: typeof(long));

        return records;
    }
}
