using System;
using Credfeto.Database.Interfaces;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Interfaces.Tests;

public sealed class SqlObjectMapAttributeTests : TestBase
{
    [Theory]
    [InlineData("sp_test", SqlObjectType.STORED_PROCEDURE, SqlDialect.GENERIC)]
    [InlineData("fn_test", SqlObjectType.SCALAR_FUNCTION, SqlDialect.MICROSOFT_SQL_SERVER)]
    [InlineData("tvf_test", SqlObjectType.TABLE_FUNCTION, SqlDialect.GENERIC)]
    public void ConstructorSetsNameProperty(string name, SqlObjectType sqlObjectType, SqlDialect sqlDialect)
    {
        SqlObjectMapAttribute attribute = new(name, sqlObjectType, sqlDialect);

        Assert.Equal(name, attribute.Name, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData("sp_test", SqlObjectType.STORED_PROCEDURE, SqlDialect.GENERIC)]
    [InlineData("fn_test", SqlObjectType.SCALAR_FUNCTION, SqlDialect.MICROSOFT_SQL_SERVER)]
    [InlineData("tvf_test", SqlObjectType.TABLE_FUNCTION, SqlDialect.GENERIC)]
    public void ConstructorSetsSqlObjectTypeProperty(string name, SqlObjectType sqlObjectType, SqlDialect sqlDialect)
    {
        SqlObjectMapAttribute attribute = new(name, sqlObjectType, sqlDialect);

        Assert.Equal(sqlObjectType, attribute.SqlObjectType);
    }

    [Theory]
    [InlineData("sp_test", SqlObjectType.STORED_PROCEDURE, SqlDialect.GENERIC)]
    [InlineData("fn_test", SqlObjectType.SCALAR_FUNCTION, SqlDialect.MICROSOFT_SQL_SERVER)]
    [InlineData("tvf_test", SqlObjectType.TABLE_FUNCTION, SqlDialect.GENERIC)]
    public void ConstructorSetsSqlDialectProperty(string name, SqlObjectType sqlObjectType, SqlDialect sqlDialect)
    {
        SqlObjectMapAttribute attribute = new(name, sqlObjectType, sqlDialect);

        Assert.Equal(sqlDialect, attribute.SqlDialect);
    }
}
