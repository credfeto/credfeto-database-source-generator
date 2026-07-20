using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Migrations.Pgsql.Tests.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Migrations.Pgsql.Tests;

public sealed class PgsqlMigrationTrackerTests : TestBase
{
    [Theory]
    [InlineData("")]
    [InlineData("1_migrations")]
    [InlineData("migrations table")]
    [InlineData("migrations-table")]
    [InlineData("migrations;drop table users")]
    public void Constructor_WithInvalidTableName_ThrowsArgumentException(string tableName)
    {
        Assert.Throws<ArgumentException>(() => new PgsqlMigrationTracker(new MigrationRunnerOptions(tableName)));
    }

    [Fact]
    public void Constructor_WithNoOptions_UsesDefaultTableName()
    {
        PgsqlMigrationTracker tracker = new();

        Assert.NotNull(tracker);
    }

    [Fact]
    public async Task EnsureCreatedAsync_CreatesTableUsingConfiguredTableName()
    {
        PgsqlMigrationTracker tracker = new(new MigrationRunnerOptions("custom_migrations"));
        await using FakeDbConnection connection = new();

        await tracker.EnsureCreatedAsync(
            connection: connection,
            cancellationToken: System.Threading.CancellationToken.None
        );

        string sql = Assert.Single(connection.ExecutedSql);
        Assert.Contains("custom_migrations", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE IF NOT EXISTS", sql, StringComparison.Ordinal);
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the connection parameter"
    )]
    public Task EnsureCreatedAsync_WithNullConnection_ThrowsArgumentNullException()
    {
        PgsqlMigrationTracker tracker = new();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .EnsureCreatedAsync(connection: null!, cancellationToken: System.Threading.CancellationToken.None)
                .AsTask()
        );
    }

    [Fact]
    public async Task GetAppliedMigrationIdsAsync_ReturnsIdsReturnedByReader()
    {
        PgsqlMigrationTracker tracker = new();
        await using FakeDbConnection connection = new(appliedIds: [1, 2, 3]);

        IReadOnlySet<long> applied = await tracker.GetAppliedMigrationIdsAsync(
            connection: connection,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(new HashSet<long> { 1, 2, 3 }, applied);
    }

    [Fact]
    public async Task GetAppliedMigrationIdsAsync_WithNoAppliedMigrations_ReturnsEmptySet()
    {
        PgsqlMigrationTracker tracker = new();
        await using FakeDbConnection connection = new();

        IReadOnlySet<long> applied = await tracker.GetAppliedMigrationIdsAsync(
            connection: connection,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Empty(applied);
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the connection parameter"
    )]
    public Task GetAppliedMigrationIdsAsync_WithNullConnection_ThrowsArgumentNullException()
    {
        PgsqlMigrationTracker tracker = new();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .GetAppliedMigrationIdsAsync(
                    connection: null!,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );
    }

    [Fact]
    public async Task RecordAppliedAsync_ExecutesInsertWithMigrationIdAndName()
    {
        PgsqlMigrationTracker tracker = new();
        await using FakeDbConnection connection = new();
        DbTransaction transaction = await connection.BeginTransactionAsync(System.Threading.CancellationToken.None);
        Migration migration = new(Id: 42, Name: "create_accounts", Sql: "CREATE TABLE accounts (id INT)");

        await tracker.RecordAppliedAsync(
            connection: connection,
            transaction: transaction,
            migration: migration,
            cancellationToken: System.Threading.CancellationToken.None
        );

        string sql = Assert.Single(connection.ExecutedSql);
        Assert.Contains("INSERT INTO", sql, StringComparison.Ordinal);
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the connection parameter"
    )]
    public Task RecordAppliedAsync_WithNullConnection_ThrowsArgumentNullException()
    {
        PgsqlMigrationTracker tracker = new();
        Migration migration = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .RecordAppliedAsync(
                    connection: null!,
                    transaction: null!,
                    migration: migration,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the transaction parameter"
    )]
    public async Task RecordAppliedAsync_WithNullTransaction_ThrowsArgumentNullException()
    {
        PgsqlMigrationTracker tracker = new();
        await using FakeDbConnection connection = new();
        Migration migration = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .RecordAppliedAsync(
                    connection: connection,
                    transaction: null!,
                    migration: migration,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the migration parameter"
    )]
    public async Task RecordAppliedAsync_WithNullMigration_ThrowsArgumentNullException()
    {
        PgsqlMigrationTracker tracker = new();
        await using FakeDbConnection connection = new();
        DbTransaction transaction = await connection.BeginTransactionAsync(System.Threading.CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .RecordAppliedAsync(
                    connection: connection,
                    transaction: transaction,
                    migration: null!,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );
    }
}
