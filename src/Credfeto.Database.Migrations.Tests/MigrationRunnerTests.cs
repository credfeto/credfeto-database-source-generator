using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Migrations.Tests.Helpers;
using FunFair.Test.Common;
using NSubstitute;
using Xunit;

namespace Credfeto.Database.Migrations.Tests;

public sealed class MigrationRunnerTests : TestBase
{
    [SuppressMessage(
        category: "Reliability",
        checkId: "CA2012:Use ValueTasks correctly",
        Justification = "NSubstitute configuration call, not a real async invocation"
    )]
    private static IMigrationTracker CreateTracker(IReadOnlySet<long>? applied = null)
    {
        IMigrationTracker tracker = GetSubstitute<IMigrationTracker>();

        tracker
            .EnsureCreatedAsync(Arg.Any<DbConnection>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(ValueTask.CompletedTask);

        tracker
            .GetAppliedMigrationIdsAsync(Arg.Any<DbConnection>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(new ValueTask<IReadOnlySet<long>>(applied ?? new HashSet<long>()));

        tracker
            .RecordAppliedAsync(
                Arg.Any<DbConnection>(),
                Arg.Any<DbTransaction>(),
                Arg.Any<IMigration>(),
                Arg.Any<System.Threading.CancellationToken>()
            )
            .Returns(ValueTask.CompletedTask);

        return tracker;
    }

    private static MigrationRunner CreateRunner(
        out FakeDbConnection connection,
        IReadOnlySet<long>? applied = null,
        Func<string, bool>? shouldFail = null
    )
    {
        connection = new(shouldFail);
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker(applied);

        return new MigrationRunner(database: database, tracker: tracker);
    }

    [Fact]
    public async Task MigrateAsync_WithMigrationsOutOfOrder_AppliesThemInIdOrder()
    {
        MigrationRunner runner = CreateRunner(out FakeDbConnection connection);

        Migration first = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration second = new(Id: 2, Name: "second", Sql: "CREATE TABLE b");

        await runner.MigrateAsync([second, first], System.Threading.CancellationToken.None);

        Assert.Equal(["CREATE TABLE a", "CREATE TABLE b"], connection.ExecutedSql);
    }

    [Fact]
    public async Task MigrateAsync_WithSuccessfulMigration_CommitsItsOwnTransaction()
    {
        MigrationRunner runner = CreateRunner(out FakeDbConnection connection);

        Migration migration = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");

        await runner.MigrateAsync([migration], System.Threading.CancellationToken.None);

        FakeDbTransaction transaction = Assert.Single(connection.Transactions);
        Assert.True(transaction.Committed, userMessage: "Transaction should have been committed");
        Assert.False(transaction.RolledBack, userMessage: "Transaction should not have been rolled back");
    }

    [Fact]
    public async Task MigrateAsync_WithFailingMigration_RollsBackAndAbortsWithoutApplyingLaterMigrations()
    {
        MigrationRunner runner = CreateRunner(
            out FakeDbConnection connection,
            shouldFail: static sql => StringComparer.Ordinal.Equals(sql, "FAIL")
        );

        Migration failing = new(Id: 1, Name: "failing", Sql: "FAIL");
        Migration later = new(Id: 2, Name: "later", Sql: "CREATE TABLE b");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.MigrateAsync([failing, later], System.Threading.CancellationToken.None).AsTask()
        );

        Assert.Equal(["FAIL"], connection.ExecutedSql);
        FakeDbTransaction transaction = Assert.Single(connection.Transactions);
        Assert.True(transaction.RolledBack, userMessage: "Transaction should have been rolled back");
        Assert.False(transaction.Committed, userMessage: "Transaction should not have been committed");
    }

    [Fact]
    public async Task MigrateAsync_WithAlreadyAppliedMigration_SkipsIt()
    {
        MigrationRunner runner = CreateRunner(out FakeDbConnection connection, applied: new HashSet<long> { 1 });

        Migration alreadyApplied = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration pending = new(Id: 2, Name: "second", Sql: "CREATE TABLE b");

        await runner.MigrateAsync([alreadyApplied, pending], System.Threading.CancellationToken.None);

        Assert.Equal(["CREATE TABLE b"], connection.ExecutedSql);
    }

    [Fact]
    public async Task MigrateAsync_WithDuplicateMigrationIds_ThrowsInvalidOperationException()
    {
        MigrationRunner runner = CreateRunner(out FakeDbConnection connection);

        Migration first = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration duplicate = new(Id: 1, Name: "duplicate", Sql: "CREATE TABLE b");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.MigrateAsync([first, duplicate], System.Threading.CancellationToken.None).AsTask()
        );

        Assert.Empty(connection.ExecutedSql);
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the migrations parameter"
    )]
    public Task MigrateAsync_WithNullMigrations_ThrowsArgumentNullException()
    {
        MigrationRunner runner = CreateRunner(out _);

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            runner.MigrateAsync(migrations: null!, cancellationToken: System.Threading.CancellationToken.None).AsTask()
        );
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the database parameter"
    )]
    public void Constructor_WithNullDatabase_ThrowsArgumentNullException()
    {
        IMigrationTracker tracker = CreateTracker();

        Assert.Throws<ArgumentNullException>(() => new MigrationRunner(database: null!, tracker: tracker));
    }

    [Fact]
    [SuppressMessage(
        category: "CSharpIsNullAnalyzer",
        checkId: "NX0002:Instance of NullForgiving operator without justification detected",
        Justification = "Verifying the ArgumentNullException.ThrowIfNull guard for the tracker parameter"
    )]
    public void Constructor_WithNullTracker_ThrowsArgumentNullException()
    {
        FakeDatabase database = new(new FakeDbConnection());

        Assert.Throws<ArgumentNullException>(() => new MigrationRunner(database: database, tracker: null!));
    }
}
