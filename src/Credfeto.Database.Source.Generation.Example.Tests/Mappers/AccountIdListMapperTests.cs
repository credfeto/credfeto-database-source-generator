using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Credfeto.Database.Source.Generation.Example.Mappers;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Mappers;

// TABLE-VALUED PARAMETER MAPPER TESTS
// These tests verify AccountIdListMapper correctly converts IReadOnlyList<AccountId>
// into a SQL Server table-valued parameter (SqlDbType.Structured).
public sealed class AccountIdListMapperTests : TestBase
{
    [Fact]
    public void MapFromDb_AlwaysThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => AccountIdListMapper.MapFromDb(new object()));
    }

    [Fact]
    public void MapToDb_WithAccountIds_SetsSqlDbTypeStructured()
    {
        IReadOnlyList<AccountId> accountIds = [new(1L), new(2L), new(3L)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        Assert.Equal(SqlDbType.Structured, parameter.SqlDbType);
    }

    [Fact]
    public void MapToDb_WithAccountIds_SetsTableTypeName()
    {
        IReadOnlyList<AccountId> accountIds = [new(1L)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        Assert.Equal("dbo.AccountId", parameter.TypeName, StringComparer.Ordinal);
    }

    [Fact]
    public void MapToDb_WithAccountIds_SetsDataTableAsValue()
    {
        IReadOnlyList<AccountId> accountIds = [new(1L), new(2L), new(3L)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        using DataTable dataTable = Assert.IsType<DataTable>(parameter.Value);
        Assert.NotNull(dataTable);
    }

    [Fact]
    public void MapToDb_WithAccountIds_DataTableHasExpectedRowCount()
    {
        IReadOnlyList<AccountId> accountIds = [new(10L), new(20L), new(30L)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        using DataTable dataTable = Assert.IsType<DataTable>(parameter.Value);
        Assert.Equal(3, dataTable.Rows.Count);
    }

    [Fact]
    public void MapToDb_WithAccountIds_DataTableHasSingleIdColumn()
    {
        IReadOnlyList<AccountId> accountIds = [new(1L)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        using DataTable dataTable = Assert.IsType<DataTable>(parameter.Value);
        DataColumn column = Assert.Single(dataTable.Columns.Cast<DataColumn>());
        Assert.Equal("Id", column.ColumnName, StringComparer.Ordinal);
        Assert.Equal(typeof(long), column.DataType);
    }

    [Fact]
    public void MapToDb_WithAccountIds_DataTableRowsContainExpectedValues()
    {
        const long FIRST_ID = 100L;
        const long SECOND_ID = 200L;
        IReadOnlyList<AccountId> accountIds = [new(FIRST_ID), new(SECOND_ID)];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        using DataTable dataTable = Assert.IsType<DataTable>(parameter.Value);
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(FIRST_ID, dataTable.Rows[0]["Id"]);
        Assert.Equal(SECOND_ID, dataTable.Rows[1]["Id"]);
    }

    [Fact]
    public void MapToDb_WithEmptyList_DataTableHasNoRows()
    {
        IReadOnlyList<AccountId> accountIds = [];
        SqlParameter parameter = new();

        AccountIdListMapper.MapToDb(value: accountIds, parameter: parameter);

        using DataTable dataTable = Assert.IsType<DataTable>(parameter.Value);
        Assert.Empty(dataTable.Rows.Cast<DataRow>());
    }
}
