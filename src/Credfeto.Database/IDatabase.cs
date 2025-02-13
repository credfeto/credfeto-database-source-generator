using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database;

public interface IDatabase
{
    ValueTask ExecuteAsync(
        Func<DbConnection, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken
    );

    ValueTask<T> ExecuteAsync<T>(
        Func<DbConnection, CancellationToken, ValueTask<T>> action,
        CancellationToken cancellationToken
    );
}
