using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database;

public interface IDatabase
{
    Task ExecuteAsync(Func<DbConnection, CancellationToken, Task> action, CancellationToken cancellationToken);

    Task<T> ExecuteAsync<T>(Func<DbConnection, CancellationToken, Task<T>> action, CancellationToken cancellationToken);
}