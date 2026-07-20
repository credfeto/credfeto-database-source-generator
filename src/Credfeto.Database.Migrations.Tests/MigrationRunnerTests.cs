using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

    [Fact]
    public async Task MigrateAsync_WithMigrationsOutOfOrder_AppliesThemInIdOrder()
    {
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker();
        MigrationRunner runner = new(database: database, tracker: tracker);

        Migration first = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration second = new(Id: 2, Name: "second", Sql: "CREATE TABLE b");

        await runner.MigrateAsync([second, first], System.Threading.CancellationToken.None);

        Assert.Equal(["CREATE TABLE a", "CREATE TABLE b"], connection.ExecutedSql);
    }

    [Fact]
    public async Task MigrateAsync_WithSuccessfulMigration_CommitsItsOwnTransaction()
    {
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker();
        MigrationRunner runner = new(database: database, tracker: tracker);

        Migration migration = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");

        await runner.MigrateAsync([migration], System.Threading.CancellationToken.None);

        FakeDbTransaction transaction = Assert.Single(connection.Transactions);
        Assert.True(transaction.Committed, userMessage: "Transaction should have been committed");
        Assert.False(transaction.RolledBack, userMessage: "Transaction should not have been rolled back");
    }

    [Fact]
    public async Task MigrateAsync_WithFailingMigration_RollsBackAndAbortsWithoutApplyingLaterMigrations()
    {
        FakeDbConnection connection = new(shouldFail: static sql => StringComparer.Ordinal.Equals(sql, "FAIL"));
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker();
        MigrationRunner runner = new(database: database, tracker: tracker);

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
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker(applied: new HashSet<long> { 1 });
        MigrationRunner runner = new(database: database, tracker: tracker);

        Migration alreadyApplied = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration pending = new(Id: 2, Name: "second", Sql: "CREATE TABLE b");

        await runner.MigrateAsync([alreadyApplied, pending], System.Threading.CancellationToken.None);

        Assert.Equal(["CREATE TABLE b"], connection.ExecutedSql);
    }

    [Fact]
    public async Task MigrateAsync_WithDuplicateMigrationIds_ThrowsInvalidOperationException()
    {
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker();
        MigrationRunner runner = new(database: database, tracker: tracker);

        Migration first = new(Id: 1, Name: "first", Sql: "CREATE TABLE a");
        Migration duplicate = new(Id: 1, Name: "duplicate", Sql: "CREATE TABLE b");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.MigrateAsync([first, duplicate], System.Threading.CancellationToken.None).AsTask()
        );

        Assert.Empty(connection.ExecutedSql);
    }

    [Fact]
    public void MigrateAsync_WithNullMigrations_ThrowsArgumentNullException()
    {
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);
        IMigrationTracker tracker = CreateTracker();
        MigrationRunner runner = new(database: database, tracker: tracker);

        MethodInfo method =
            typeof(MigrationRunner).GetMethod(nameof(MigrationRunner.MigrateAsync))
            ?? throw new InvalidOperationException("Cannot find MigrateAsync method");

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(obj: runner, parameters: [null, System.Threading.CancellationToken.None])
        );

        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullDatabase_ThrowsArgumentNullException()
    {
        IMigrationTracker tracker = CreateTracker();

        ConstructorInfo constructor =
            typeof(MigrationRunner).GetConstructor([typeof(IDatabase), typeof(IMigrationTracker)])
            ?? throw new InvalidOperationException("Cannot find MigrationRunner constructor");

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
            constructor.Invoke([null, tracker])
        );

        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullTracker_ThrowsArgumentNullException()
    {
        FakeDbConnection connection = new();
        FakeDatabase database = new(connection);

        ConstructorInfo constructor =
            typeof(MigrationRunner).GetConstructor([typeof(IDatabase), typeof(IMigrationTracker)])
            ?? throw new InvalidOperationException("Cannot find MigrationRunner constructor");

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
            constructor.Invoke([database, null])
        );

        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }
}
