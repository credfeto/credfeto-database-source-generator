using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Migrations.TestHelpers;
using Credfeto.Database.Migrations.Tests.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Migrations.Tests;

public sealed class MigrationTrackerBaseTests : TestBase
{
    [Theory]
    [InlineData("")]
    [InlineData("1_migrations")]
    [InlineData("migrations table")]
    [InlineData("migrations-table")]
    [InlineData("migrations;drop table users")]
    public static void Constructor_WithInvalidTableName_ThrowsArgumentException(string tableName)
    {
        Assert.Throws<ArgumentException>(() => new TestMigrationTracker(new MigrationRunnerOptions(tableName)));
    }

    [Theory]
    [InlineData("_migrations")]
    [InlineData("Migrations2")]
    [InlineData("custom_migrations")]
    public static void Constructor_WithValidTableName_UsesConfiguredTableName(string tableName)
    {
        TestMigrationTracker tracker = new(new MigrationRunnerOptions(tableName));

        Assert.Equal(expected: tableName, actual: tracker.ConfiguredTableName);
    }

    [Fact]
    public void Constructor_WithNoOptions_UsesDefaultTableName()
    {
        TestMigrationTracker tracker = new();

        Assert.Equal(expected: "__migrations", actual: tracker.ConfiguredTableName);
    }

    [Fact]
    public async Task EnsureCreatedAsync_ExecutesEnsureCreatedSql()
    {
        TestMigrationTracker tracker = new(new MigrationRunnerOptions("custom_migrations"));
        await using FakeDbConnection connection = new();

        await tracker.EnsureCreatedAsync(
            connection: connection,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(["ENSURE custom_migrations"], connection.ExecutedSql);
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the connection parameter"
    )]
    public Task EnsureCreatedAsync_WithNullConnection_ThrowsArgumentNullException()
    {
        TestMigrationTracker tracker = new();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            tracker
                .EnsureCreatedAsync(connection: null!, cancellationToken: System.Threading.CancellationToken.None)
                .AsTask()
        );
    }

    [Fact]
    public async Task GetAppliedMigrationIdsAsync_ReturnsIdsReturnedByReader()
    {
        TestMigrationTracker tracker = new();
        await using FakeDbConnection connection = new(appliedIds: [3, 1, 2]);

        IReadOnlySet<long> applied = await tracker.GetAppliedMigrationIdsAsync(
            connection: connection,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(new HashSet<long> { 1, 2, 3 }, applied);
        Assert.Equal(["SELECT __migrations"], connection.ExecutedSql);
    }

    [Fact]
    public async Task GetAppliedMigrationIdsAsync_WithNoAppliedMigrations_ReturnsEmptySet()
    {
        TestMigrationTracker tracker = new();
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
        TestMigrationTracker tracker = new();

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
    public async Task RecordAppliedAsync_ExecutesInsertSqlWithoutCompletingTransaction()
    {
        TestMigrationTracker tracker = new();
        await using FakeDbConnection connection = new();
        DbTransaction transaction = await connection.BeginTransactionAsync(System.Threading.CancellationToken.None);
        Migration migration = new(Id: 42, Name: "create_accounts", Sql: "CREATE TABLE accounts (id INT)");

        await tracker.RecordAppliedAsync(
            connection: connection,
            transaction: transaction,
            migration: migration,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(["INSERT __migrations"], connection.ExecutedSql);
        FakeDbTransaction recorded = Assert.Single(connection.Transactions);
        Assert.False(recorded.Committed, userMessage: "Tracker should leave committing to the caller");
        Assert.False(recorded.RolledBack, userMessage: "Tracker should not roll back the caller's transaction");
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the connection parameter"
    )]
    public Task RecordAppliedAsync_WithNullConnection_ThrowsArgumentNullException()
    {
        TestMigrationTracker tracker = new();
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
        TestMigrationTracker tracker = new();
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
        TestMigrationTracker tracker = new();
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
