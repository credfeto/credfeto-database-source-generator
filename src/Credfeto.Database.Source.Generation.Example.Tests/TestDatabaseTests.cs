using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database;
using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests;

public sealed class TestDatabaseTests : TestBase
{
    private static AccountAddress CreateAddress()
    {
        return new() { Value = "0x1234567890abcdef" };
    }

    private static Accounts CreateAccount(AccountAddress address)
    {
        return new(
            Id: 1,
            Name: "Test Account",
            Address: address,
            LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
        );
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToDatabase_ReturnsExpected()
    {
        AccountAddress address = CreateAddress();
        IReadOnlyList<Accounts> expected = [CreateAccount(address)];

        TestDatabase sut = new(new PresetDatabase(expected));

        IReadOnlyList<Accounts> result = await sut.GetAllAsync(
            accountAddress: address,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetAsync_DelegatesToDatabase_ReturnsExpected()
    {
        AccountAddress address = CreateAddress();
        Accounts expected = CreateAccount(address);

        TestDatabase sut = new(new PresetDatabase(expected));

        Accounts? result = await sut.GetAsync(id: 1, cancellationToken: System.Threading.CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task InsertAsync_DelegatesToDatabase()
    {
        AccountAddress address = CreateAddress();

        PresetDatabase db = new(null);
        TestDatabase sut = new(db);

        await sut.InsertAsync(
            name: "Test",
            address: address,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(1, db.ExecuteCount);
    }

    [Fact]
    public async Task GetMeaningOfLifeAsync_DelegatesToDatabase_ReturnsExpected()
    {
        const int EXPECTED = 42;

        TestDatabase sut = new(new PresetDatabase(EXPECTED));

        int result = await sut.GetMeaningOfLifeAsync(System.Threading.CancellationToken.None);

        Assert.Equal(EXPECTED, result);
    }

    [Fact]
    public async Task GetOptionalMeaningOfLifeAsync_DelegatesToDatabase_ReturnsExpected()
    {
        int? expected = 42;

        TestDatabase sut = new(new PresetDatabase(expected));

        int? result = await sut.GetOptionalMeaningOfLifeAsync(System.Threading.CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetStringMeaningOfLifeAsync_DelegatesToDatabase_ReturnsExpected()
    {
        const string EXPECTED = "forty-two";

        TestDatabase sut = new(new PresetDatabase(EXPECTED));

        string result = await sut.GetStringMeaningOfLifeAsync(System.Threading.CancellationToken.None);

        Assert.Equal(EXPECTED, result, StringComparer.Ordinal);
    }

    [Fact]
    public async Task GetAddressMeaningOfLifeAsync_DelegatesToDatabase_ReturnsExpected()
    {
        AccountAddress expected = CreateAddress();

        TestDatabase sut = new(new PresetDatabase(expected));

        AccountAddress result = await sut.GetAddressMeaningOfLifeAsync(System.Threading.CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetOptionalAddressMeaningOfLifeAsync_DelegatesToDatabase_ReturnsExpected()
    {
        AccountAddress expected = CreateAddress();

        TestDatabase sut = new(new PresetDatabase(expected));

        AccountAddress? result = await sut.GetOptionalAddressMeaningOfLifeAsync(
            System.Threading.CancellationToken.None
        );

        Assert.Same(expected, result);
    }

    [Fact]
    public Task GetAllAsync_WithExecutingDatabase_ExecutesLambda()
    {
        AccountAddress address = CreateAddress();
        DbConnection connection = GetSubstitute<DbConnection>();

        TestDatabase sut = new(new ExecutingDatabase(connection));

        return Assert.ThrowsAnyAsync<Exception>(() =>
            sut.GetAllAsync(accountAddress: address, cancellationToken: System.Threading.CancellationToken.None)
                .AsTask()
        );
    }

    [Fact]
    public Task GetAsync_WithExecutingDatabase_ExecutesLambda()
    {
        DbConnection connection = GetSubstitute<DbConnection>();

        TestDatabase sut = new(new ExecutingDatabase(connection));

        return Assert.ThrowsAnyAsync<Exception>(() =>
            sut.GetAsync(id: 1, cancellationToken: System.Threading.CancellationToken.None).AsTask()
        );
    }

    [Fact]
    public Task InsertAsync_WithExecutingDatabase_ExecutesLambda()
    {
        AccountAddress address = CreateAddress();
        DbConnection connection = GetSubstitute<DbConnection>();

        TestDatabase sut = new(new ExecutingDatabase(connection));

        return Assert.ThrowsAnyAsync<Exception>(() =>
            sut.InsertAsync(name: "Test", address: address, cancellationToken: System.Threading.CancellationToken.None)
                .AsTask()
        );
    }

    // TABLE-VALUED PARAMETER DELEGATION TESTS

    [Fact]
    public async Task BulkGetAccountsByIdsAsync_DelegatesToDatabase_ReturnsExpected()
    {
        AccountAddress address = CreateAddress();
        IReadOnlyList<Accounts> expected = [CreateAccount(address)];
        IReadOnlyList<AccountId> accountIds = [new(1L), new(2L)];

        TestDatabase sut = new(new PresetDatabase(expected));

        IReadOnlyList<Accounts> result = await sut.BulkGetAccountsByIdsAsync(
            accountIds: accountIds,
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Same(expected, result);
    }

    [Fact]
    public Task BulkGetAccountsByIdsAsync_WithExecutingDatabase_ExecutesLambda()
    {
        IReadOnlyList<AccountId> accountIds = [new(1L)];
        DbConnection connection = GetSubstitute<DbConnection>();

        TestDatabase sut = new(new ExecutingDatabase(connection));

        return Assert.ThrowsAnyAsync<Exception>(() =>
            sut.BulkGetAccountsByIdsAsync(
                    accountIds: accountIds,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );
    }

    private sealed class PresetDatabase : IDatabase
    {
        private readonly object? _returnValue;

        public PresetDatabase(object? returnValue)
        {
            this._returnValue = returnValue;
        }

        public int ExecuteCount { get; private set; }

        public ValueTask ExecuteAsync(
            Func<DbConnection, CancellationToken, ValueTask> action,
            CancellationToken cancellationToken
        )
        {
            this.ExecuteCount++;

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> ExecuteAsync<T>(
            Func<DbConnection, CancellationToken, ValueTask<T>> action,
            CancellationToken cancellationToken
        )
        {
            if (this._returnValue is null)
            {
                throw new InvalidOperationException(message: "No return value configured for typed execution.");
            }

            return ValueTask.FromResult((T)this._returnValue);
        }
    }

    private sealed class ExecutingDatabase : IDatabase
    {
        private readonly DbConnection _connection;

        public ExecutingDatabase(DbConnection connection)
        {
            this._connection = connection;
        }

        public ValueTask ExecuteAsync(
            Func<DbConnection, CancellationToken, ValueTask> action,
            CancellationToken cancellationToken
        )
        {
            return action(this._connection, cancellationToken);
        }

        public ValueTask<T> ExecuteAsync<T>(
            Func<DbConnection, CancellationToken, ValueTask<T>> action,
            CancellationToken cancellationToken
        )
        {
            return action(this._connection, cancellationToken);
        }
    }
}
