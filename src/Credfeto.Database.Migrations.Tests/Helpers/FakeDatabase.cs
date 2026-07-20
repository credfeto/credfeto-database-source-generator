using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Migrations.TestHelpers;
using Polly;

namespace Credfeto.Database.Migrations.Tests.Helpers;

internal sealed class FakeDatabase : BaseDatabase
{
    private readonly FakeDbConnection _connection;

    public FakeDatabase(FakeDbConnection connection)
    {
        this._connection = connection;
    }

    protected override bool IsTransientException(Exception exception)
    {
        return false;
    }

    protected override void LogAndDispatchTransientExceptions(
        Exception exception,
        Context context,
        in TimeSpan delay,
        int retryCount,
        int maxRetries
    ) { }

    protected override ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<DbConnection>(this._connection);
    }
}
