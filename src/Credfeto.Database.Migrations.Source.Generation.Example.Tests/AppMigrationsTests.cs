using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Database.Migrations;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Migrations.Source.Generation.Example.Tests;

public sealed class AppMigrationsTests : TestBase
{
    [Fact]
    public void Migrations_GeneratedFromSqlFiles_AreOrderedById()
    {
        IReadOnlyList<IMigration> migrations = AppMigrations.Migrations;

        Assert.Equal([1L, 2L, 3L], migrations.Select(static migration => migration.Id));
    }

    [Fact]
    public void Migrations_GeneratedFromSqlFiles_HaveNamesParsedFromTheirFileNames()
    {
        IReadOnlyList<IMigration> migrations = AppMigrations.Migrations;

        Assert.Equal(
            ["create_accounts", "add_email_column", "create_email_index"],
            migrations.Select(static migration => migration.Name)
        );
    }

    [Fact]
    public void Migrations_CreateAccountsMigration_ContainsTheSqlFromItsFile()
    {
        IMigration migration = AppMigrations.Migrations.Single(static candidate => candidate.Id == 1);

        Assert.Contains("CREATE TABLE Dbo.Accounts", migration.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Migrations_AddEmailColumnMigration_ContainsTheSqlFromItsFile()
    {
        IMigration migration = AppMigrations.Migrations.Single(static candidate => candidate.Id == 2);

        Assert.Contains("ADD Email NVARCHAR(200) NULL", migration.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Migrations_CreateEmailIndexMigration_ContainsTheSqlFromItsFile()
    {
        IMigration migration = AppMigrations.Migrations.Single(static candidate => candidate.Id == 3);

        Assert.Contains("CREATE INDEX Ix_Accounts_Email ON Dbo.Accounts", migration.Sql, StringComparison.Ordinal);
    }
}
