using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Tests.Helpers;
using FunFair.Test.Common;
using NSubstitute;
using Xunit;

namespace Credfeto.Database.Tests;

public sealed class BaseDatabaseTests : TestBase
{
    private static TestDatabase CreateInstance(
        IEnumerable<Func<CancellationToken, ValueTask<DbConnection>>> connectionResponses
    )
    {
        return new TestDatabase(connectionResponses);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulGetConnection_CallsAction()
    {
        DbConnection connection = GetSubstitute<DbConnection>();
        bool actionCalled = false;
        TestDatabase db = CreateInstance([_ => ValueTask.FromResult(connection)]);

        await db.ExecuteAsync(
            action: (_, _) =>
            {
                actionCalled = true;

                return ValueTask.CompletedTask;
            },
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.True(actionCalled, userMessage: "Action should have been called");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonTransientException_ThrowsWithoutLogging()
    {
        TestDatabase db = CreateInstance([_ => throw new InvalidOperationException(message: "Non-transient error")]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            db.ExecuteAsync(
                    action: (_, _) => ValueTask.CompletedTask,
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );

        Assert.Equal(expected: 0, actual: db.LogCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithTransientExceptionThenSuccess_LogsAndRetries()
    {
        DbConnection connection = GetSubstitute<DbConnection>();
        bool actionCalled = false;

        TestDatabase db = CreateInstance(
            [_ => throw new TransientTestException(), _ => ValueTask.FromResult(connection)]
        );

        await db.ExecuteAsync(
            action: (_, _) =>
            {
                actionCalled = true;

                return ValueTask.CompletedTask;
            },
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(expected: 1, actual: db.LogCallCount);
        Assert.True(actionCalled, userMessage: "Action should have been called after retry");
    }

    [Fact]
    public async Task ExecuteAsync_Generic_WithSuccessfulGetConnection_ReturnsExpectedResult()
    {
        const int EXPECTED_RESULT = 42;
        DbConnection connection = GetSubstitute<DbConnection>();
        TestDatabase db = CreateInstance([_ => ValueTask.FromResult(connection)]);

        int result = await db.ExecuteAsync<int>(
            action: (_, _) => ValueTask.FromResult(EXPECTED_RESULT),
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(expected: EXPECTED_RESULT, actual: result);
    }

    [Fact]
    public async Task ExecuteAsync_Generic_WithNonTransientException_Throws()
    {
        TestDatabase db = CreateInstance([_ => throw new InvalidOperationException(message: "Non-transient error")]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            db.ExecuteAsync<int>(
                    action: (_, _) => ValueTask.FromResult(0),
                    cancellationToken: System.Threading.CancellationToken.None
                )
                .AsTask()
        );

        Assert.Equal(expected: 0, actual: db.LogCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_Generic_WithTransientExceptionThenSuccess_ReturnsExpectedResult()
    {
        const int EXPECTED_RESULT = 99;
        DbConnection connection = GetSubstitute<DbConnection>();

        TestDatabase db = CreateInstance(
            [_ => throw new TransientTestException(), _ => ValueTask.FromResult(connection)]
        );

        int result = await db.ExecuteAsync<int>(
            action: (_, _) => ValueTask.FromResult(EXPECTED_RESULT),
            cancellationToken: System.Threading.CancellationToken.None
        );

        Assert.Equal(expected: 1, actual: db.LogCallCount);
        Assert.Equal(expected: EXPECTED_RESULT, actual: result);
    }
}
