using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Credfeto.Database.Tests.Helpers;

internal sealed class TestDatabase : BaseDatabase
{
    private readonly Queue<Func<CancellationToken, ValueTask<DbConnection>>> _connectionResponses;

    public TestDatabase(IEnumerable<Func<CancellationToken, ValueTask<DbConnection>>> connectionResponses)
    {
        this._connectionResponses = new(connectionResponses);
    }

    public int LogCallCount { get; private set; }

    protected override bool IsTransientException(Exception exception)
    {
        return exception is TransientTestException;
    }

    protected override void LogAndDispatchTransientExceptions(
        Exception exception,
        Context context,
        in TimeSpan delay,
        int retryCount,
        int maxRetries
    )
    {
        this.LogCallCount++;
    }

    protected override ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (this._connectionResponses.TryDequeue(out Func<CancellationToken, ValueTask<DbConnection>>? response))
        {
            return response(cancellationToken);
        }

        throw new InvalidOperationException(message: "No more connection responses configured");
    }
}
